using AlephNote.WPF.MVVM;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;
using System.Windows.Input;
using AlephNote.Common.MVVM;
using AlephNote.WPF.Dialogs;

namespace AlephNote.WPF.Windows
{
	public class UpdateWindowViewmodel : ObservableObject
	{
		public ICommand UpdateManualCommand { get { return new RelayCommand(UpdateManual); } }
		public ICommand UpdateAutoCommand { get { return new RelayCommand(UpdateAuto); } }
		public ICommand CloseCommand { get { return new RelayCommand(() => _owner.Close()); } }

		private string _versionLocal = App.AppVersionProperty;
		public string VersionLocal { get { return _versionLocal; } set { _versionLocal = value; OnPropertyChanged(); } }

		private string _versionOnline;
		public string VersionOnline { get { return _versionOnline; } set { _versionOnline = value; OnPropertyChanged(); } }

		private DateTime _dateOnline;
		public DateTime DateOnline { get { return _dateOnline; } set { _dateOnline = value; OnPropertyChanged(); } }

		private bool _buttonsEnabled = true;
		public bool ButtonsEnabled { get { return _buttonsEnabled; } set { _buttonsEnabled = value; OnPropertyChanged(); } }

		private int _progressValue = 0;
		public int ProgressValue { get { return _progressValue; } set { _progressValue = value; OnPropertyChanged(); } }

		private int _progressMax = 100;
		public int ProgressMax { get { return _progressMax; } set { _progressMax = value; OnPropertyChanged(); } }

		public string URL = "";
		private string _savePath;

		private readonly UpdateWindow _owner;

		public UpdateWindowViewmodel(UpdateWindow w)
		{
			_owner = w;
		}

		private void UpdateManual()
		{
			var sfd = new SaveFileDialog
			{
				FileName = Path.GetFileName(URL) ?? "AlephNoteUpdate.zip",
				Filter = "Zip files (*.zip)|*.zip"
			};

			if (sfd.ShowDialog(_owner) != true) return;

			ButtonsEnabled = false;

			_savePath = sfd.FileName;

			WebClient webClient = new WebClient();
			webClient.DownloadFileCompleted += (s, e) => Application.Current.Dispatcher.BeginInvoke(new Action(() => CompletedManual(e)));
			webClient.DownloadProgressChanged += ProgressChanged;
			webClient.DownloadFileAsync(new Uri(URL), _savePath);
		}

		private void UpdateAuto()
		{
			ButtonsEnabled = false;

			_savePath = Path.GetTempFileName() + ".zip";

			WebClient webClient = new WebClient();
			webClient.DownloadFileCompleted += (s, e) => Application.Current.Dispatcher.BeginInvoke(new Action(() => CompletedAuto(e)));
			webClient.DownloadProgressChanged += ProgressChanged;
			webClient.DownloadFileAsync(new Uri(URL), _savePath);
		}

		private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			Application.Current.Dispatcher.BeginInvoke(new Action(() => { ProgressValue = e.ProgressPercentage; }));
		}

		private void CompletedManual(AsyncCompletedEventArgs args)
		{
			if (args.Error != null)
			{
				ExceptionDialog.Show(_owner, "Could not download new version", args.Error, "");
				ButtonsEnabled = true;
				return;
			}

			_owner.Close();

			Process.Start(_savePath);
		}

		private void CompletedAuto(AsyncCompletedEventArgs args)
		{
			if (args.Error != null)
			{
				ExceptionDialog.Show(_owner, "Could not download new version", args.Error, "");
				ButtonsEnabled = true;
				return;
			}

			try
			{
				var folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("B"));

				Directory.CreateDirectory(folder);

				ZipFile.ExtractToDirectory(_savePath, folder);

				var updater = Path.Combine(folder, @"AutoUpdater.exe");

				Process.Start(new ProcessStartInfo(updater, '"' + AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + '"'));

				_owner.Close();
				_owner.MainViewmodel.Exit();
			}
			catch (Exception e)
			{
				ExceptionDialog.Show(_owner, "Could not auto update", e, "");
			}

		}
	}
}
