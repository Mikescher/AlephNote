using AlephNote.PluginInterface;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using AlephNote.Common.Repository;
using AlephNote.Common.Settings;
using AlephNote.PluginInterface.Util;
using AlephNote.Common.Util;
using System.Collections.Generic;
using AlephNote.PluginInterface.AppContext;
using MSHC.Util.Helper;
using ProcessHelper = MSHC.Util.Helper.ProcessHelper;

namespace AlephNote.Common.Operations
{
	public static class LocalGitBackup
	{
		public static string CommitMessageHeader = "Automatic Mirroring of AlephNote notes";
		public static string CommitMessageProgName = "AlephNote";
		
		private static readonly object _gitAccessLock = new object();

		private class AugmentedNote { public INote Note; public string RelativePath; public string Content; }

		public static void UpdateRepository(NoteRepository nrepo, AppSettings config)
		{
			if (!config.DoGitMirror) return;

			if (string.IsNullOrWhiteSpace(config.GitMirrorPath))
			{
				LoggerSingleton.Inst.Warn("LocalGitMirror", "Cannot do a local git mirror: Path is empty.");
				return;
			}
			if (string.IsNullOrWhiteSpace(config.GitMirrorFirstName))
			{
				LoggerSingleton.Inst.Warn("LocalGitMirror", "Cannot do a local git mirror: Authorname is empty.");
				return;
			}
			if (string.IsNullOrWhiteSpace(config.GitMirrorLastName))
			{
				LoggerSingleton.Inst.Warn("LocalGitMirror", "Cannot do a local git mirror: Authorname is empty.");
				return;
			}
			if (string.IsNullOrWhiteSpace(config.GitMirrorMailAddress))
			{
				LoggerSingleton.Inst.Warn("LocalGitMirror", "Cannot do a local git mirror: Authormail is empty.");
				return;
			}

			try
			{
				lock(_gitAccessLock)
				{
					var notes = GetNotes(nrepo);

					if (!NeedsUpdate(notes, config))
					{
						LoggerSingleton.Inst.Debug("LocalGitMirror", "git repository is up to date - no need to commit");
						return;
					}

					Directory.CreateDirectory(config.GitMirrorPath);

					var githead = Path.Combine(config.GitMirrorPath, ".git", "HEAD");
					if (! File.Exists(githead))
					{
						if (Directory.EnumerateFileSystemEntries(config.GitMirrorPath).Any())
						{
							LoggerSingleton.Inst.Warn("LocalGitMirror", "Cannot do a local git mirror: Targetfolder is neither a repository nor empty.");
							return;
						}

						var o = ProcessHelper.ProcExecute("git", "init", config.GitMirrorPath);
						LoggerSingleton.Inst.Debug("LocalGitMirror", "git mirror [git init]", o.ToString());
					}

					string targetFolder = config.GitMirrorPath;
					
					if (config.GitMirrorSubfolders)
					{
						var subfolder = Path.Combine(config.GitMirrorPath, config.ActiveAccount.ID.ToString("B"));
						Directory.CreateDirectory(subfolder);
						targetFolder = subfolder;
					}

					var changed = SyncNotesToFolder(notes, targetFolder);

					if (!changed)
					{
						LoggerSingleton.Inst.Info("LocalGitMirror", "Local git synchronisation was triggered but no changes were found");
						return;
					}
				}

				new Thread(() =>
				{
					var succ = CommitRepository(
						nrepo.ConnectionName + " (" + nrepo.ConnectionUUID + ")",
						nrepo.ProviderID,
						config.GitMirrorPath, 
						config.GitMirrorFirstName, 
						config.GitMirrorLastName, 
						config.GitMirrorMailAddress, 
						config.GitMirrorDoPush);

					if (succ && config.GitMirrorAutoGC > 0)
					{
						CollectGarbage(config.GitMirrorPath, config.GitMirrorAutoGC);
					}

				}).Start();

			}
			catch (Exception e)
			{
				LoggerSingleton.Inst.Error("LocalGitMirror", "Local git mirroring failed with exception:\n" + e.Message, e.ToString());
				LoggerSingleton.Inst.ShowExceptionDialog("Local git mirror failed", e);
			}
		}

		private static void CollectGarbage(string repoPath, int gcCount)
		{
			try
			{
				var cache = LoadGitGCCache();
				
				if (!cache.ContainsKey(repoPath)) cache[repoPath] = 0;

				cache[repoPath] = cache[repoPath] + 1;

				if (cache[repoPath] >= gcCount)
				{
					cache[repoPath] = 0;
					LoggerSingleton.Inst.Debug("LocalGitMirror", $"Resetting GC counter to {cache[repoPath]}/{gcCount}", "");
					SaveGitGCCache(cache);
					
					// https://stackoverflow.com/a/18006967/1761622
					// stupid git - supressing output if stderr is not a tty
					var o1 = ProcessHelper.ProcExecute("git", "gc", repoPath);
					LoggerSingleton.Inst.Debug("LocalGitMirror", "git mirror [git gc]", o1.ToString());
				}
				else
				{
					LoggerSingleton.Inst.Debug("LocalGitMirror", $"Increment GC counter to {cache[repoPath]}/{gcCount}", "");
					SaveGitGCCache(cache);
				}

			}
			catch (Exception e)
			{
				LoggerSingleton.Inst.Error("LocalGitMirror", "Could not load GC_CACHE file", e);
				LoggerSingleton.Inst.ShowExceptionDialog("Local git mirror (git gc) failed", e);
			}
		}

		private static Dictionary<string, int> LoadGitGCCache()
		{
			if (!File.Exists(AppSettings.PATH_GCCACHE)) return new Dictionary<string, int>();
				
			var cache = new Dictionary<string, int>();

			foreach (var line in FileSystemUtil.ReadAllLinesSafe(AppSettings.PATH_GCCACHE))
			{
				if (string.IsNullOrWhiteSpace(line)) continue;
				var split = line.Split('\t');
				if (split.Length != 2) continue;
				if (split[0].Length<2 || !split[0].StartsWith("[") || !split[0].EndsWith("]")) continue;

				var key = split[0].Substring(1, split[0].Length-2);

				if (int.TryParse(split[1], out var val)) cache[key] = val;
			}

			return cache;
		}

		private static void SaveGitGCCache(Dictionary<string, int> data)
		{
			File.WriteAllLines(AppSettings.PATH_GCCACHE, data.Select(p => $"[{p.Key}]\t{p.Value}"));
		}

		private static List<AugmentedNote> GetNotes(NoteRepository repo)
		{
			var result = new List<AugmentedNote>();

			var existing = new HashSet<string>();

			foreach (var note in repo.Notes.OrderBy(p => p.CreationDate))
			{
				var fn = ANFilenameHelper.ConvertStringForFilename(note.Title);
				if (string.IsNullOrWhiteSpace(fn)) fn = ANFilenameHelper.StripStringForFilename(note.UniqueName);

				var ext = ".txt";

				if (note.HasTagCaseInsensitive(AppSettings.TAG_MARKDOWN)) ext = ".md";

				var path = Path.Combine(note.Path.Enumerate().Select(c => ANFilenameHelper.StripStringForFilename(c)).ToArray());

				var oldfn = fn;

				int idx = 0;
				string rpath = "";
				for (;;)
				{
					fn = (idx == 0) ? oldfn : $"{oldfn}_{idx:000}";

					rpath = string.IsNullOrWhiteSpace(path) ? (fn+ext) : Path.Combine(path, fn+ext);

					if (existing.Add(rpath)) break;

					idx++;
				}
				
				string txt = note.Text;
				if (fn != note.Title && !string.IsNullOrWhiteSpace(note.Title)) txt = note.Title + "\n\n" + note.Text;

				result.Add(new AugmentedNote(){ Note = note, RelativePath = rpath, Content = txt });
			}

			return result;
		}

		private static bool SyncNotesToFolder(List<AugmentedNote> reponotes, string targetFolder)
		{
			var dataRepo   = reponotes.ToList();
			var dataSystem = new List<Tuple<string, string>>(); // <rel_path, content>

			foreach (var file in FileSystemUtil.EnumerateFilesDeep(targetFolder, 16, new[]{".git"}))
			{
				if (!(file.ToLower().EndsWith(".txt") || file.ToLower().EndsWith(".md"))) continue;
				
				var rpath = FileSystemUtil.MakePathRelative(file, targetFolder);
				var txt   = File.ReadAllText(file, Encoding.UTF8);

				dataSystem.Add(Tuple.Create(rpath, txt));
			}

			var files_nochange = new List<string>();
			for (int i = dataRepo.Count-1; i >= 0; i--)
			{
				var match = dataSystem.FirstOrDefault(ds => ds.Item1 == dataRepo[i].RelativePath && ds.Item2 == dataRepo[i].Content);
				if (match == null) continue;

				// Note exists in repo and filesystem with same content - everything is ok

				files_nochange.Add(match.Item1);

				dataSystem.Remove(match);
				dataRepo.RemoveAt(i);
			}

			if (dataSystem.Count==0 && dataRepo.Count==0) 
			{
				LoggerSingleton.Inst.Debug("LocalGitMirror", "SyncNotesToFolder found 0 differences", string.Join("\n", files_nochange));
				return false;
			}
			
			var files_deleted = new List<string>();
			for (int i = dataSystem.Count-1; i >= 0; i--)
			{
				if (dataRepo.Any(ds => ds.RelativePath == dataSystem[i].Item1)) continue;

				// Note exists in filesystem but not in repo - delete it

				//var noteRepo = null;
				var noteFSys = dataSystem[i];

				files_deleted.Add(noteFSys.Item1);

				dataSystem.RemoveAt(i);
				var fpath = Path.Combine(targetFolder, noteFSys.Item1);
				File.Delete(fpath);

				LoggerSingleton.Inst.Info("LocalGitMirror", $"File deleted: '{noteFSys.Item1}'", fpath);
			}
			
			var files_modified = new List<string>();
			for (int i = dataSystem.Count-1; i >= 0; i--)
			{
				var match = dataRepo.FirstOrDefault(ds => ds.RelativePath == dataSystem[i].Item1 && ds.Content == dataSystem[i].Item2);
				if (match == null) continue;

				// Note exists in filesystem and in repo but with different content - modify it
				
				var noteRepo = match;
				var noteFSys = dataSystem[i];

				files_modified.Add(noteFSys.Item1);
				dataSystem.RemoveAt(i);
				dataRepo.Remove(noteRepo);

				var fpath = Path.Combine(targetFolder, noteRepo.RelativePath);
				File.WriteAllText(fpath, noteRepo.Content, new UTF8Encoding(false));
				
				LoggerSingleton.Inst.Info("LocalGitMirror", $"File modified: '{noteRepo.RelativePath}'", fpath);
			}
			
			var files_created = new List<string>();
			for (int i = dataRepo.Count-1; i >= 0; i--)
			{
				// Note exists in repo but not in filesystem - create it
				
				var noteRepo = dataRepo[i];
				//var noteFSys = null;

				files_created.Add(noteRepo.RelativePath);
				dataRepo.RemoveAt(i);

				var fpath = Path.Combine(targetFolder, noteRepo.RelativePath);
				Directory.CreateDirectory(Path.GetDirectoryName(fpath));
				File.WriteAllText(fpath, noteRepo.Content, new UTF8Encoding(false));
				
				LoggerSingleton.Inst.Info("LocalGitMirror", $"File created: '{noteRepo.RelativePath}'", fpath);
			}
			
			var dir_deleted = new List<string>();
			foreach (var dir in FileSystemUtil.EnumerateEmptyDirectories(targetFolder, 16).ToList())
			{
				dir_deleted.Add(dir);
				ANFileSystemUtil.DeleteDirectoryWithRetry(LoggerSingleton.Inst, dir);
				
				var rpath = FileSystemUtil.MakePathRelative(dir, targetFolder);
				LoggerSingleton.Inst.Info("LocalGitMirror", $"Directory dleted: '{rpath}'", dir);
			}

			LoggerSingleton.Inst.Debug("LocalGitMirror", "SyncNotesToFolder found multiple differences", 
				"Unchanged:\n{\n"         + string.Join("\n", files_nochange.Select(p => "    "+p)) + "\n}\n\n" + 
				"Deleted:\n{\n"           + string.Join("\n", files_deleted.Select(p  => "    "+p)) + "\n}\n\n" + 
				"Created:\n{\n"           + string.Join("\n", files_created.Select(p  => "    "+p)) + "\n}\n\n" + 
				"Modified:\n{\n"          + string.Join("\n", files_modified.Select(p => "    "+p)) + "\n}\n\n" + 
				"Empty-Directories:\n{\n" + string.Join("\n", dir_deleted.Select(p    => "    "+p)) + "\n}\n\n");

			return true;
		}

		private static bool NeedsUpdate(List<AugmentedNote> notes, AppSettings config)
		{
			if (!File.Exists(Path.Combine(config.GitMirrorPath, ".git", "HEAD"))) return true;

			var folder = config.GitMirrorPath;
			if (config.GitMirrorSubfolders) folder = Path.Combine(config.GitMirrorPath, config.ActiveAccount.ID.ToString("B"));

			if (!Directory.Exists(folder)) return true;
			
			var filesGit  = FileSystemUtil
				.EnumerateFilesDeep(folder, 8)
				.Where(f => f.ToLower().EndsWith(".txt") || f.ToLower().EndsWith(".md"))
				.Select(Path.GetFileName)
				.Select(f => f.ToLower())
				.ToList();

			var filesThis = notes
				.Select(p => Path.Combine(folder, p.RelativePath))
				.Select(f => f.ToLower())
				.ToList();

			if (filesGit.Count != filesThis.Count) return true;
			if (filesGit.Except(filesThis).Any()) return true;
			if (filesThis.Except(filesGit).Any()) return true;

			foreach (var note in notes)
			{
				try
				{
					var fn = Path.Combine(folder, note.RelativePath);
					var txtThis = note.Content;
					var txtGit = File.ReadAllText(fn, Encoding.UTF8);

					if (txtGit != txtThis) return true;
				}
				catch(IOException)
				{
					return true;
				}
			}

			return false;
		}

		private static bool CommitRepository(string provname, string provid, string repoPath, string firstname, string lastname, string mail, bool pushremote)
		{
			try
			{
				lock (_gitAccessLock)
				{
					var o1 = ProcessHelper.ProcExecute("git", "add .", repoPath);
					if (o1.ExitCode != 0)
					{
						LoggerSingleton.Inst.Error("LocalGitMirror", "git mirror [git add] failed", o1.ToString());
						return false;
					}
					else
					{
						LoggerSingleton.Inst.Debug("LocalGitMirror", "git mirror [git add]", o1.ToString());
					}

					var o2 = ProcessHelper.ProcExecute("git", "status", repoPath);
					LoggerSingleton.Inst.Debug("LocalGitMirror", "git mirror [git status]", o2.ToString());
					if (o2.StdOut.Contains("nothing to commit") || o2.StdErr.Contains("nothing to commit"))
					{
						LoggerSingleton.Inst.Debug("LocalGitMirror", "Local git mirror not updated ('nothing to commit')");
						return false;
					}
					var msg =
						CommitMessageHeader + "\n" +
						"" + "\n" +
						"# "+CommitMessageProgName+" Version: " + $"{AlephAppContext.AppVersion.Item1}.{AlephAppContext.AppVersion.Item2}.{AlephAppContext.AppVersion.Item3}.{AlephAppContext.AppVersion.Item4}" + "\n" +
						"# Timestamp(UTC): " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\n" +
						"# Provider: " + provname + "\n" +
						"# Provider (ID): " + provid + "\n" +
						"# Hostname: " + System.Environment.MachineName + "\n";

					var o3 = ProcessHelper.ProcExecute("git", $"commit -a --allow-empty --message=\"{msg}\" --author=\"{firstname} {lastname} <{mail}>\"", repoPath);
					if (o3.ExitCode != 0)
					{
						LoggerSingleton.Inst.Error("LocalGitMirror", "git mirror [git commit] failed", o3.ToString());
						return false;
					}
					else
					{
						LoggerSingleton.Inst.Debug("LocalGitMirror", "git mirror [git commit]", o3.ToString());
					}

					if (pushremote)
					{
						var o4 = ProcessHelper.ProcExecute("git", "push", repoPath);
						if (o4.ExitCode != 0)
						{
							LoggerSingleton.Inst.Error("LocalGitMirror", "git mirror [git push] failed", o4.ToString());
							return false;
						}
						else
						{
							LoggerSingleton.Inst.Debug("LocalGitMirror", "git mirror [git push]", o4.ToString());
						}
					}

					LoggerSingleton.Inst.Info("LocalGitMirror", "Local git mirror updated" + (pushremote ? " (+ pushed)":""));
					return true;
				}
			}
			catch (Exception e)
			{
				LoggerSingleton.Inst.Error("PluginManager", "Local git mirroring (commit) failed with exception:\n" + e.Message, e.ToString());
				LoggerSingleton.Inst.ShowExceptionDialog("Local git mirror (commit) failed", e);
				return false;
			}
		}
	}
}
