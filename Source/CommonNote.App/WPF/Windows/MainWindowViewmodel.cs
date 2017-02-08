using CommonNote.Settings;
using MSHC.MVVM;
using System.Windows.Input;

namespace CommonNote.WPF.Windows
{
	public class MainWindowViewmodel : ObservableObject
	{
		public ICommand SettingsCommand { get { return new RelayCommand(ShowSettings); } }

		private CommonNoteSettings _settings;
		public CommonNoteSettings Settings { get { return _settings; } private set { _settings = value; OnPropertyChanged(); } }

		public MainWindowViewmodel(CommonNoteSettings settings)
		{
			_settings = settings;
		}

		private void ShowSettings()
		{
			new SettingsWindow(this, Settings).ShowDialog();
		}

		public void ChangeSettings(CommonNoteSettings newSettings)
		{
			Settings = newSettings;
			Settings.Save();
		}
	}
}
