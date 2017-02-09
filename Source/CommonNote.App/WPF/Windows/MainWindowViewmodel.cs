using System.Linq;
using CommonNote.PluginInterface;
using CommonNote.Repository;
using CommonNote.Settings;
using MSHC.WPF.MVVM;
using System;
using System.Windows;
using System.Windows.Input;

namespace CommonNote.WPF.Windows
{
	public class MainWindowViewmodel : ObservableObject
	{
		public ICommand SettingsCommand { get { return new RelayCommand(ShowSettings); } }
		public ICommand CreateNewNoteCommand { get { return new RelayCommand(CreateNote);} }

		private AppSettings _settings;
		public AppSettings Settings { get { return _settings; } private set { _settings = value; OnPropertyChanged(); } }

		private NoteRepository _repository;
		public NoteRepository Repository { get { return _repository; } private set { _repository = value; OnPropertyChanged(); } }

		private INote _selectedNote;
		public INote SelectedNote { get { return _selectedNote; } private set { _selectedNote = value; OnPropertyChanged(); } }

		public MainWindowViewmodel(AppSettings settings)
		{
			_settings = settings;
			_repository = new NoteRepository(App.PATH_LOCALDB, settings.NoteProvider, settings.PluginSettings[settings.NoteProvider.GetUniqueID()]);

			Repository.Init();

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
				_repository = new NoteRepository(App.PATH_LOCALDB, Settings.NoteProvider, Settings.PluginSettings[Settings.NoteProvider.GetUniqueID()]);
				_repository.Init();

				OnExplicitPropertyChanged("Repository");

				SelectedNote = Repository.Notes.FirstOrDefault();
			}
		}
	}
}
