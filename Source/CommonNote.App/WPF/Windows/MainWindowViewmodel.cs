using System.ComponentModel;
using System.IO;
using System.Text;
using CommonNote.PluginInterface;
using CommonNote.Repository;
using CommonNote.Settings;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;
using MSHC.WPF.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CommonNote.WPF.Windows
{
	public class MainWindowViewmodel : ObservableObject, ISynchronizationFeedback
	{
		public ICommand SettingsCommand { get { return new RelayCommand(ShowSettings); } }
		public ICommand CreateNewNoteCommand { get { return new RelayCommand(CreateNote);} }
		public ICommand ResyncCommand { get { return new RelayCommand(Resync); } }
		public ICommand ShowMainWindowCommand { get { return new RelayCommand(ShowMainWindow); } }
		public ICommand ExportCommand { get { return new RelayCommand(ExportNote); } }
		public ICommand DeleteCommand { get { return new RelayCommand(DeleteNote); } }
		public ICommand ExitCommand { get { return new RelayCommand(Exit); } }

		public ICommand ClosingEvent { get { return new RelayCommand<CancelEventArgs>(OnClosing); } }
		public ICommand CloseEvent { get { return new RelayCommand<EventArgs>(OnClose); } }
		public ICommand StateChangedEvent { get { return new RelayCommand<EventArgs>(OnStateChanged); } }

		private AppSettings _settings;
		public AppSettings Settings { get { return _settings; } private set { _settings = value; OnPropertyChanged(); } }

		private NoteRepository _repository;
		public NoteRepository Repository { get { return _repository; } private set { _repository = value; OnPropertyChanged(); } }

		private INote _selectedNote;
		public INote SelectedNote { get { return _selectedNote; } private set { _selectedNote = value; OnPropertyChanged(); SelectedNoteChanged(); } }

		private DateTimeOffset? _lastSynchronized = null;

		private string _lastSynchronizedText = "never";
		public string LastSynchronizedText { get { return _lastSynchronizedText; } set { _lastSynchronizedText = value; OnPropertyChanged(); } }

		private WindowState _windowState = WindowState.Normal;
		public WindowState WindowState { get { return _windowState; } set { _windowState = value; OnPropertyChanged(); } }

		private bool _forceClose = false;

		public readonly MainWindow Owner;

		public MainWindowViewmodel(AppSettings settings, MainWindow parent)
		{
			_settings = settings;
			Owner = parent;
			_repository = new NoteRepository(App.PATH_LOCALDB, this, settings, settings.NoteProvider, settings.PluginSettings[settings.NoteProvider.GetUniqueID()]);

			Repository.Init();
			Owner.TrayIcon.Visibility = (Settings.CloseToTray || Settings.MinimizeToTray) ? Visibility.Visible : Visibility.Collapsed;

			SelectedNote = Repository.Notes.FirstOrDefault();
		}

		private void ShowSettings()
		{
			new SettingsWindow(this, Settings).ShowDialog();
		}

		private void CreateNote()
		{
			try
			{
				Repository.CreateNewNote();
			}
			catch (Exception e)
			{
				MessageBox.Show("Cannot create note cause of " + e.Message + "\r\n\r\n" + e, "CreateNote failed");
			}
		}

		public void ChangeSettings(AppSettings newSettings)
		{
			var reconnectRepo = Settings.NoteProvider != newSettings.NoteProvider || !Settings.PluginSettings[Settings.NoteProvider.GetUniqueID()].IsEqual(newSettings.PluginSettings[newSettings.NoteProvider.GetUniqueID()]);

			if (reconnectRepo)
			{
				_repository.Shutdown();
			}

			Settings = newSettings;
			Settings.Save();

			if (reconnectRepo)
			{
				_repository = new NoteRepository(App.PATH_LOCALDB, this, Settings, Settings.NoteProvider, Settings.PluginSettings[Settings.NoteProvider.GetUniqueID()]);
				_repository.Init();

				OnExplicitPropertyChanged("Repository");

				SelectedNote = Repository.Notes.FirstOrDefault();
			}

			Owner.TrayIcon.Visibility = (Settings.CloseToTray || Settings.MinimizeToTray) ? Visibility.Visible : Visibility.Collapsed;


			Owner.SetupScintilla(Settings);
		}

		private void SelectedNoteChanged()
		{
			Owner.ResetScintillaScroll();
			Owner.FocusScintilla();
		}

		private void Resync()
		{
			Repository.SyncNow();
		}

		public void StartSync()
		{
			LastSynchronizedText = "[SYNCING]";
		}

		public void SyncSuccess(DateTimeOffset now)
		{
			_lastSynchronized = now;
			LastSynchronizedText = now.ToLocalTime().ToString("HH:mm:ss");
		}

		public void SyncError(List<Tuple<string, Exception>> errors)
		{
			Owner.TrayIcon.ShowBalloonTip("Synchronization failed", string.Join(Environment.NewLine, errors.Select(p => p.Item1)), BalloonIcon.Error);

			if (_lastSynchronized != null)
			{
				LastSynchronizedText = _lastSynchronized.Value.ToLocalTime().ToString("HH:mm:ss");
			}
			else
			{
				LastSynchronizedText = "[ERROR]";
			}
		}

		public void ShowMainWindow()
		{
			Owner.Show();
			WindowState = WindowState.Normal;
			Owner.Activate();
			Owner.Focus();
		}

		private void OnClosing(CancelEventArgs e)
		{
			if (Settings.CloseToTray && !_forceClose)
			{
				Owner.Hide();
				e.Cancel = true;
			}
		}

		private void OnClose(EventArgs e)
		{
			Repository.Shutdown();
		}

		private void OnStateChanged(EventArgs e)
		{
			if (WindowState == WindowState.Minimized && Settings.MinimizeToTray)
			{
				Owner.Hide();
			}
		}

		private void ExportNote()
		{
			if (SelectedNote == null) return;

			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Filter = "Text files (*.txt)|*.txt";
			sfd.FileName = SelectedNote.Title + ".txt";

			if (sfd.ShowDialog() == true)
			{
				try
				{
					File.WriteAllText(sfd.FileName, SelectedNote.Text, Encoding.UTF8);
				}
				catch (Exception)
				{
					MessageBox.Show(Owner, "Could not write to file", "Error in WriteAllText");
				}
			}
		}

		private void DeleteNote()
		{
			if (SelectedNote == null) return;

			Repository.DeleteNote(SelectedNote, true);

			SelectedNote = Repository.Notes.FirstOrDefault();
		}

		private void Exit()
		{
			_forceClose = true;
			Owner.Close();
		}
	}
}
