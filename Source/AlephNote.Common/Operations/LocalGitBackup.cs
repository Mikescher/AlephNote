using AlephNote.Common.Extensions;
using AlephNote.PluginInterface;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using AlephNote.Common.Repository;
using AlephNote.Common.Settings;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Common.Operations
{
	public static class LocalGitBackup
	{
		private static readonly object _gitAccessLock = new object();

		public static void UpdateRepository(NoteRepository repo, AppSettings config, IAlephLogger logger)
		{
			if (!config.DoGitMirror) return;

			if (string.IsNullOrWhiteSpace(config.GitMirrorPath))
			{
				logger.Warn("LocalGitMirror", "Cannot do a local git mirror: Path is empty.");
				return;
			}
			if (string.IsNullOrWhiteSpace(config.GitMirrorFirstName))
			{
				logger.Warn("LocalGitMirror", "Cannot do a local git mirror: Authorname is empty.");
				return;
			}
			if (string.IsNullOrWhiteSpace(config.GitMirrorLastName))
			{
				logger.Warn("LocalGitMirror", "Cannot do a local git mirror: Authorname is empty.");
				return;
			}
			if (string.IsNullOrWhiteSpace(config.GitMirrorMailAddress))
			{
				logger.Warn("LocalGitMirror", "Cannot do a local git mirror: Authormail is empty.");
				return;
			}

			try
			{
				lock(_gitAccessLock)
				{
					if (!NeedsUpdate(repo, config))
					{
						logger.Debug("LocalGitMirror", "git repository is up to date - no need to commit");
						return;
					}

					Directory.CreateDirectory(config.GitMirrorPath);

					var githead = Path.Combine(config.GitMirrorPath, ".git", "HEAD");
					if (! File.Exists(githead))
					{
						if (Directory.EnumerateFileSystemEntries(config.GitMirrorPath).Any())
						{
							logger.Warn("LocalGitMirror", "Cannot do a local git mirror: Targetfolder is neither a repository nor empty.");
							return;
						}

						var o = ProcessHelper.ProcExecute("git", "init", config.GitMirrorPath);
						logger.Debug("LocalGitMirror", "git mirror [git init]", o.ToString());
					}

					if (config.GitMirrorSubfolders)
					{
						CleanFolderFromNotes(config.GitMirrorPath);

						var subfolder = Path.Combine(config.GitMirrorPath, config.ActiveAccount.ID.ToString("B"));
						Directory.CreateDirectory(subfolder);

						CleanFolderFromNotes(subfolder);

						foreach (var note in repo.Notes)
						{
							var fn = GetFilename(note);
							var path = Path.Combine(subfolder, fn);
							var fld = Path.GetDirectoryName(path);
							var txt = GetFileContent(note);

							if (fld != null) Directory.CreateDirectory(fld);
							File.WriteAllText(path, txt, new UTF8Encoding(false));
						}
					}
					else
					{
						CleanFolderFromNotes(config.GitMirrorPath);

						foreach (var note in repo.Notes)
						{
							var fn = GetFilename(note);
							var path = Path.Combine(config.GitMirrorPath, fn);
							var fld = Path.GetDirectoryName(path);
							var txt = GetFileContent(note);

							if (fld != null) Directory.CreateDirectory(fld);
							File.WriteAllText(path, txt, new UTF8Encoding(false));
						}
					}
				}

				new Thread(() =>
				{
					CommitRepository(
						repo.ConnectionName + " (" + repo.ConnectionUUID + ")",
						repo.ProviderID,
						config.GitMirrorPath, 
						config.GitMirrorFirstName, 
						config.GitMirrorLastName, 
						config.GitMirrorMailAddress, 
						config.GitMirrorDoPush, 
						logger);
				}).Start();

			}
			catch (Exception e)
			{
				logger.Error("PluginManager", "Local git mirroring failed with exception:\n" + e.Message, e.ToString());
				logger.ShowExceptionDialog("Local git mirror failed", e);
			}
		}

		private static void CleanFolderFromNotes(string folder)
		{
			foreach (var file in Directory.EnumerateFiles(folder))
			{
				if (file.ToLower().EndsWith(".txt") || file.ToLower().EndsWith(".md")) File.Delete(file);
			}

			foreach (var dir in Directory.EnumerateDirectories(folder))
			{
				if ((Path.GetFileName(dir) ?? "").ToLower() == ".git") continue;
				Directory.Delete(dir, true);
			}
		}

		private static string GetFilename(INote note)
		{
			var fn = FilenameHelper.ConvertStringForFilename(note.Title);
			if (string.IsNullOrWhiteSpace(fn)) fn = FilenameHelper.StripStringForFilename(note.GetUniqueName());

			var ext = ".txt";

			if (note.HasTagCasInsensitive(AppSettings.TAG_MARKDOWN)) ext = ".md";

			var path = Path.Combine(note.Path.Enumerate().Select(c => FilenameHelper.StripStringForFilename(c)).ToArray());

			if (string.IsNullOrWhiteSpace(path)) return fn + ext;

			return Path.Combine(path, fn + ext);
		}

		private static string GetFileContent(INote note)
		{
			var fn = GetFilename(note);

			string txt = note.Text;
			if (fn != (note.Title + ".txt") && !string.IsNullOrWhiteSpace(note.Title))
			{
				txt = note.Title + "\n\n" + note.Text;
			}
			return txt;
		}

		private static bool NeedsUpdate(NoteRepository repo, AppSettings config)
		{
			if (!File.Exists(Path.Combine(config.GitMirrorPath, ".git", "HEAD"))) return true;

			var folder = config.GitMirrorPath;
			if (config.GitMirrorSubfolders) folder = Path.Combine(config.GitMirrorPath, config.ActiveAccount.ID.ToString("B"));

			if (!Directory.Exists(folder)) return true;
			
			var filesGit  = FileSystemUtil
				.EnumerateFilesDeep(folder, 8)
				.Where(f => f.ToLower().EndsWith(".txt") || f.ToLower().EndsWith(".md"))
				.Select(Path.GetFileName).Select(f => f.ToLower()).ToList();

			var filesThis = repo.Notes.Select(GetFilename).Select(f => f.ToLower()).ToList(); //TODO What happens when 2 notes have same title

			if (filesGit.Count != filesThis.Count) return true;
			if (filesGit.Except(filesThis).Any()) return true;
			if (filesThis.Except(filesGit).Any()) return true;

			foreach (var note in repo.Notes)
			{
				try
				{
					var fn = Path.Combine(folder, GetFilename(note));
					var txtThis = GetFileContent(note);
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

		private static void CommitRepository(string provname, string provid, string repoPath, string firstname, string lastname, string mail, bool pushremote, IAlephLogger logger)
		{
			try
			{
				lock (_gitAccessLock)
				{
					var o1 = ProcessHelper.ProcExecute("git", "add .", repoPath);
					logger.Debug("LocalGitMirror", "git mirror [git add]", o1.ToString());

					var o2 = ProcessHelper.ProcExecute("git", "status", repoPath);
					logger.Debug("LocalGitMirror", "git mirror [git status]", o2.ToString());
					if (o2.StdOut.Contains("nothing to commit") || o2.StdErr.Contains("nothing to commit"))
					{
						logger.Debug("LocalGitMirror", "Local git mirror not updated ('nothing to commit')");
						return;
					}
					var msg =
						"Automatic Mirroring of AlephNote notes" + "\n" +
						"" + "\n" +
						"# AlephNote Version: " + logger.AppVersion + "\n" +
						"# Timestamp(UTC): " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\n" +
						"# Provider: " + provname + "\n" +
						"# Provider (ID): " + provid + "\n";

					var o3 = ProcessHelper.ProcExecute("git", $"commit -a --allow-empty --message=\"{msg}\" --author=\"{firstname} {lastname} <{mail}>\"", repoPath);
					logger.Debug("LocalGitMirror", "git mirror [git commit]", o3.ToString());

					if (pushremote)
					{
						var o4 = ProcessHelper.ProcExecute("git", "push", repoPath);
						logger.Debug("LocalGitMirror", "git mirror [git push]", o4.ToString());
					}

					logger.Info("LocalGitMirror", "Local git mirror updated" + (pushremote ? " (+ pushed)":""));
				}
			}
			catch (Exception e)
			{
				logger.Error("PluginManager", "Local git mirroring (commit) failed with exception:\n" + e.Message, e.ToString());
				logger.ShowExceptionDialog("Local git mirror (commit) failed", e);
			}
		}
	}
}
