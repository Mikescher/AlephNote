using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;

namespace AlephNote.AutoUpdater
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly string targetPath;
		private readonly string sourcePath;

		private static readonly string[] FILEPATTERN =
		{
			"AlephNote.exe",
			"*.dll",
			"Plugins/*.dll",
		};

		public MainWindow()
		{
			InitializeComponent();

			var cmdparams = Environment.GetCommandLineArgs();

			if (cmdparams.Length <= 1) Fail("Invalid call arguments");

			sourcePath = AppDomain.CurrentDomain.BaseDirectory;
			targetPath = cmdparams[1];

			if (!Directory.Exists(targetPath)) Fail("Target path not found");
			if (!Directory.Exists(sourcePath)) Fail("Source path not found");

			new Thread(Run).Start();
		}

		private void Fail(string msg)
		{
			MessageBox.Show("Auto Update failed - " + msg, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
			Environment.Exit(-1);
		}

		private void Run()
		{
			try
			{
				Update();
			}
			catch (Exception e)
			{
				Fail("Internal Error\r\n" + e.Message);
			}
		}

		private void Update()
		{
			var d = Application.Current.Dispatcher;

			d.Invoke(() => InfoBox.Text = "Enumerate files");

			List<Tuple<string, string>> files = FILEPATTERN
				.SelectMany(pattern => Directory
					.EnumerateFiles(sourcePath, pattern)
					.Select(file => Tuple.Create(file, Path.Combine(targetPath, MakeRelative(file, sourcePath)))))
				.ToList();


			d.Invoke(() => Progress.Maximum = files.Count + 2);
			d.Invoke(() => Progress.Value = 1);
			d.Invoke(() => InfoBox.Text = "Stop running process");

			var processes = Process.GetProcessesByName("AlephNote");
			while (processes.Length > 0)
			{
				var messageResult = processes[0].CloseMainWindow();

				if (messageResult)
				{
					Thread.Sleep(100);
					processes = Process.GetProcessesByName("AlephNote");
					continue;
				}

				Thread.Sleep(500);

				if (processes[0].HasExited)
				{
					processes = Process.GetProcessesByName("AlephNote");
					continue;
				}

				//TODO Exit gracefully

				Thread.Sleep(500);

				processes[0].Kill();

				Thread.Sleep(500);

				processes = Process.GetProcessesByName("AlephNote");
			}

			d.Invoke(() => Progress.Value = 1);

			foreach (var ffile in files)
			{
				var file = ffile;

				d.Invoke(() => InfoBox.Text = Path.GetFileName(file.Item1));

				var start = Environment.TickCount;

				try
				{
					File.Copy(file.Item1, file.Item2, true);
				}
				catch (Exception)
				{
					Fail("Could not copy " + Path.GetFileName(file.Item1));
				}

				var delta = 133 - (Environment.TickCount - start);
				if (delta > 0) Thread.Sleep(delta);
			}

			Process.Start(Path.Combine(targetPath, "AlephNote.exe"));

			d.Invoke(Close);
		}

		static public String MakeRelative(String fromPath, String baseDir)
		{
			const string pathSep = "\\";
			string[] p1 = Regex.Split(fromPath, "[\\\\/]").Where(x => x.Length != 0).ToArray();
			string[] p2 = Regex.Split(baseDir, "[\\\\/]").Where(x => x.Length != 0).ToArray();

			int i = 0;
			for (; i < p1.Length && i < p2.Length; i++)
			{
				if (string.Compare(p1[i], p2[i], StringComparison.OrdinalIgnoreCase) != 0) break;
			}

			string r = string.Join(pathSep, Enumerable.Repeat("..", p1.Length - i - 1).Concat(p2.Skip(i).Take(p2.Length - i)));

			return (r.Length == 0) ? p1.Last() : String.Join(pathSep, r, p1.Last());
		}
	}
}
