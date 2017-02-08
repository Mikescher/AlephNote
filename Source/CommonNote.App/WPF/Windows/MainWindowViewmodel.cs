using CommonNote.Repository;
using CommonNote.Settings;
using System.Windows.Input;
using MSHC.WPF.MVVM;

namespace CommonNote.WPF.Windows
{
	public class MainWindowViewmodel : ObservableObject
	{
		public ICommand SettingsCommand { get { return new RelayCommand(ShowSettings); } }

		private AppSettings _settings;
		public AppSettings Settings { get { return _settings; } private set { _settings = value; OnPropertyChanged(); } }
		
		private NoteRepository _repository;
		public NoteRepository Repository { get { return _repository; } private set { _repository = value; OnPropertyChanged(); } }

		public MainWindowViewmodel(AppSettings settings)
		{
			_settings = settings;
			_repository = new NoteRepository(App.PATH_LOCALDB, settings.NoteProvider, settings.PluginSettings[settings.NoteProvider.GetUniqueID()]);

			Repository.Init();
		}

		private void ShowSettings()
		{
			new SettingsWindow(this, Settings).ShowDialog();
		}

		public void ChangeSettings(AppSettings newSettings)
		{
			Settings = newSettings;
			Settings.Save();
		}
	}
}
