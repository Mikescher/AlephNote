using AlephNote.Common.Extensions;
using AlephNote.PluginInterface;
using AlephNote.Repository;
using AlephNote.Settings;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace AlephNote.Common.Operations
{
	public static class LocalGitBackup
	{
		private static object _gitAccessLock = new object();

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

					foreach (var file in Directory.EnumerateFiles(config.GitMirrorPath))
					{
						if (file.ToLower().EndsWith(".txt")) File.Delete(file);
					}
					foreach (var note in repo.Notes)
					{
						var fn = FilenameHelper.StripStringForFilename(note.Title);
						if (string.IsNullOrWhiteSpace(fn)) fn = FilenameHelper.StripStringForFilename(note.GetUniqueName());

						string txt = note.Text;
						if (fn != note.Title && !string.IsNullOrWhiteSpace(note.Title))
						{
							txt = note.Title + "\n\n" + note.Text;
						}

						File.WriteAllText(Path.Combine(config.GitMirrorPath, fn + ".txt"), txt, new UTF8Encoding(false));
					}
				}

				new Thread(() =>
				{
					CommitRepository(
						repo.ConnectionName,
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
					if (o2.StdOut.Contains("nothing to commit") || o2.StdErr.Contains("nothing to commit")) return;

					var msg =
						"Automatic Mirroring of AlephNote notes" + "\n" +
						"" + "\n" +
						"# AlephNote Version: " + logger.AppVersion + "\n" +
						"# Timestamp(UTC): " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\n" +
						"# Provider: " + provname + "\n" +
						"# Provider (ID): " + provid + "\n";

					var o3 = ProcessHelper.ProcExecute("git", $"commit -a --allow-empty --message=\"{msg}\" --author=\"AlephNote Git <auto@example.com>\"", repoPath);
					logger.Debug("LocalGitMirror", "git mirror [git commit]", o3.ToString());

					if (pushremote)
					{
						var o4 = ProcessHelper.ProcExecute("git", "push", repoPath);
						logger.Debug("LocalGitMirror", "git mirror [git push]", o4.ToString());
					}
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
