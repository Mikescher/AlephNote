using AlephNote.PluginInterface;
using AlephNote.WPF.MVVM;
using System.Windows;
using System.Windows.Input;
using System;
using System.Linq;
using AlephNote.Common.MVVM;
using AlephNote.Common.Settings;
using AlephNote.Common.Settings.Types;
using AlephNote.WPF.Shortcuts;
using System.Collections.Generic;
using AlephNote.Common.Themes;

namespace AlephNote.WPF.Windows
{
	class SettingsWindowViewmodel : ObservableObject
	{
		private readonly MainWindow mainWindow;

		public AppSettings Settings { get; private set; }
		
		public ICommand InsertCurrentWindowStateCommand { get { return new RelayCommand(InsertCurrentWindowState); } }

		public ObservableCollectionNoReset<ObservableShortcutConfig> ShortcutList { get; set; }

		private ObservableShortcutConfig _selectedShortcut;
		public ObservableShortcutConfig SelectedShortcut { get { return _selectedShortcut; } set { _selectedShortcut = value; OnPropertyChanged(); } }

		public List<AlephTheme> AvailableThemes { get; set; }

		private AlephTheme _selectedTheme;
		public AlephTheme SelectedTheme { get { return _selectedTheme; } set { _selectedTheme = value; OnPropertyChanged(); } }

		public SettingsWindowViewmodel(MainWindow main, AppSettings data)
		{
			mainWindow = main;
			Settings = data;

			ShortcutList = ShortcutManager.ListObservableShortcuts(data);
			AvailableThemes = App.Themes.GetAllAvailable();

			SelectedTheme = App.Themes.GetByFilename(Settings.Theme, out _) 
						 ?? App.Themes.GetByFilename("default.xml", out _)
						 ?? AvailableThemes.FirstOrDefault()
						 ?? App.Themes.GetDefault();
		}

		public void OnBeforeApply()
		{
			var sdata = ShortcutList
				.Where(s => s.Key != AlephKey.None)
				.Select(s => Tuple.Create(s.Identifier, new ShortcutDefinition(s.Scope, s.Modifiers, s.Key)));

			Settings.Shortcuts = new KeyValueFlatCustomList<ShortcutDefinition>(sdata, ShortcutDefinition.DEFAULT);

			Settings.Theme = SelectedTheme.SourceFilename;
		}

		private void InsertCurrentWindowState()
		{
			if (mainWindow.WindowState == WindowState.Maximized)
			{
				Settings.StartupLocation = ExtendedWindowStartupLocation.CenterScreen;
				Settings.StartupState = ExtendedWindowState.Maximized;
				Settings.StartupPositionX = (int)mainWindow.Left;
				Settings.StartupPositionY = (int)mainWindow.Top;
				Settings.StartupPositionWidth = (int)mainWindow.Width;
				Settings.StartupPositionHeight = (int)mainWindow.Height;
			}
			else if (mainWindow.WindowState == WindowState.Minimized)
			{
				Settings.StartupLocation = ExtendedWindowStartupLocation.Manual;
				Settings.StartupState = ExtendedWindowState.Minimized;
				Settings.StartupPositionX = (int)mainWindow.Left;
				Settings.StartupPositionY = (int)mainWindow.Top;
				Settings.StartupPositionWidth = (int)mainWindow.Width;
				Settings.StartupPositionHeight = (int)mainWindow.Height;
			}
			else if (mainWindow.WindowState == WindowState.Normal)
			{
				Settings.StartupLocation = ExtendedWindowStartupLocation.Manual;
				Settings.StartupState = ExtendedWindowState.Normal;
				Settings.StartupPositionX = (int)mainWindow.Left;
				Settings.StartupPositionY = (int)mainWindow.Top;
				Settings.StartupPositionWidth = (int)mainWindow.Width;
				Settings.StartupPositionHeight = (int)mainWindow.Height;
			}
		}

		public void AddAccount(IRemotePlugin p)
		{
			var acc = new RemoteStorageAccount(Guid.NewGuid(), p, p.CreateEmptyRemoteStorageConfiguration());

			Settings.AddAccountAndSetActive(acc);
		}

		public void RemoveAccount()
		{
			if (Settings.Accounts.Count <= 1) return;

			Settings.RemoveAccount(Settings.ActiveAccount);
		}
	}
}
