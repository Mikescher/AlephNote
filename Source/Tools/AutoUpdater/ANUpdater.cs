using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace AlephNote.AutoUpdater
{
	public class ANUpdater
	{
		private static readonly string[] FILEPATTERN =
		{
			"AlephNote.exe",
			"*.dll",
			"Plugins/*.dll",
			"Themes/*.xml",
		};

		private readonly Action<int, int, string> _setState;
		private readonly Action<string, bool> _showMessage;
		private readonly Action<string> _fail;
		private readonly string _sourcePath;
		private readonly string _targetPath;

		private bool _deleteNotesFolder = false;

		public ANUpdater(Action<int, int, string> setFull, Action<string> fail, Action<string, bool> showMsg, string src, string dst)
		{
			_setState = setFull;
			_showMessage = showMsg;
			_fail = fail;
			_sourcePath = src;
			_targetPath = dst;
		}

		public void Run()
		{
			_setState(0, 100, "Enumerating files");

			List<Tuple<string, string>> files = FILEPATTERN
				.SelectMany(pattern => Directory
					.EnumerateFiles(Path.Combine(new[] { _sourcePath }.Concat(pattern.Split('/').Reverse().Skip(1).Reverse()).ToArray()), pattern.Split('/').Last())
					.Where(f => (Path.GetFileNameWithoutExtension(f) ?? "").ToLower() != "autoupdater")
					.Select(file => Tuple.Create(file, Path.Combine(_targetPath, MakeRelative(file, _sourcePath)))))
				.ToList();

			int max = 0;
			max += 1;           // stop runnign progresses
			max += 1;           // migrating
			max += files.Count; // copy
			max += 1;           // repo-migration
			max += 1;           // restarting

			int progress = 1;

			_setState(progress++, max, "Stop running process");
			KillProcess();

			_setState(progress++, max, "Migration");
			Migrate(out var versionOld);

			foreach (var ffile in files)
			{
				var file = ffile;

				_setState(progress++, max, Path.GetFileName(file.Item1));
				
				var start = Environment.TickCount;

				try
				{
					Directory.CreateDirectory(Path.GetDirectoryName(file.Item2) ?? "");
					File.Copy(file.Item1, file.Item2, true);
				}
				catch (Exception)
				{
					_fail("Could not copy " + Path.GetFileName(file.Item1));
				}

				var delta = 333 - (Environment.TickCount - start);
				if (delta > 0) Thread.Sleep(delta);
			}

			var versionNew = File.Exists(Path.Combine(_targetPath, "AlephNote.exe"))
				? new Version(FileVersionInfo.GetVersionInfo(Path.Combine(_targetPath, "AlephNote.exe")).FileVersion)
				: null;

			_setState(progress++, max, "Migrate repository");
			RepoMigrate();

			_setState(progress++, max, "Restarting");
			Process.Start(Path.Combine(_targetPath, "AlephNote.exe"), $"--updated --migration_from={versionOld?.ToString()??"NULL"} --migration_to={versionNew?.ToString()??"NULL"}");
			Thread.Sleep(500);
		}

		private void Migrate(out Version versionOld)
		{
			var exe = Path.Combine(_targetPath, "AlephNote.exe");
			if (!File.Exists(exe))
			{
				_showMessage("AlephNote.exe in targetfolder not found - cannot run migrations", true);
				Thread.Sleep(300);
				versionOld = null;
				return;
			}

			var fvi = FileVersionInfo.GetVersionInfo(exe);

			var version = new Version(fvi.FileVersion);
			versionOld = version;

			if (version <= new Version("1.4.1.0"))
			{
				_showMessage(
					"You are upgrading from an version <= 1.4.1.0\n" +
					"In this version there were some changes to the account managment.\n" +
					"If you continue you will have to re-add your account data in the AlephNote settings and all notes have to be downloaded again.\n" +
					"If your notes aren't synchronized with an remote this can lead to data-loss.\n", 
					true);

				_deleteNotesFolder = true;
			}

			if (version <= new Version("1.6.2.0"))
			{
				try
				{
					// from 1.6.2.0 the key contains the userID
					// delete the old key
					var registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
					if (registryKey?.GetValue("AlephNoteApp") != null) registryKey.DeleteValue("AlephNoteApp");

					_showMessage(
						"You are upgrading from an version <= 1.6.2.0\n" +
						"The autostart settings have changed, if you want to start AlephNote on system boot please set the option in the settings again.\n",
						false);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}

			Thread.Sleep(300);
		}

		private void KillProcess()
		{
			var processes = Process.GetProcessesByName("AlephNote");
			if (processes.Length > 1) Thread.Sleep(1000);

			while ((processes = Process.GetProcessesByName("AlephNote")).Length > 0)
			{
				var messageResult = processes[0].CloseMainWindow();

				if (messageResult)
				{
					Thread.Sleep(100);
					continue;
				}

				Thread.Sleep(500);

				if (processes[0].HasExited) continue;

				processes[0].Kill();

				Thread.Sleep(500);
			}
		}

		private void RepoMigrate()
		{
			if (_deleteNotesFolder)
			{
				var folder = Path.Combine(_targetPath, ".notes");
				var di = new DirectoryInfo(folder);
				if (!di.Exists) return;

				di.Delete(true);
			}
		}

		private String MakeRelative(String fromPath, String baseDir)
		{
			const string pathSep = "\\";
			string[] p1 = Regex.Split(fromPath, "[\\\\/]").Where(x => x.Length != 0).ToArray();
			string[] p2 = Regex.Split(baseDir, "[\\\\/]").Where(x => x.Length != 0).ToArray();

			int i = 0;
			for (; i < p1.Length && i < p2.Length; i++)
			{
				if (string.Compare(p1[i], p2[i], StringComparison.OrdinalIgnoreCase) != 0) break;
			}

			return string.Join(pathSep, p1.Skip(i));
		}
	}
}
