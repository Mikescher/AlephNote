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
using AlephNote.Common.Util;
using AlephNote.WPF.Util;

namespace AlephNote.WPF.Windows
{
	class SettingsWindowViewmodel : ObservableObject
	{
		private readonly MainWindow mainWindow;

		public AppSettings Settings { get; private set; }

		public bool IsAppDebugMode => App.DebugMode;

		public ICommand InsertCurrentWindowStateCommand { get { return new RelayCommand(InsertCurrentWindowState); } }

		public ObservableCollectionNoReset<ObservableShortcutConfig> ShortcutList { get; set; }

		private ObservableShortcutConfig _selectedShortcut;
		public ObservableShortcutConfig SelectedShortcut { get { return _selectedShortcut; } set { _selectedShortcut = value; OnPropertyChanged(); } }

		public List<AlephTheme> AvailableThemes { get; set; }

		private AlephTheme _selectedTheme;
		public AlephTheme SelectedTheme { get { return _selectedTheme; } set { _selectedTheme = value; OnPropertyChanged(); UpdateThemePreview(); } }

		private AlephTheme _oldTheme = null;
		private bool _isThemePreview = false;

		public SettingsWindowViewmodel(MainWindow main, AppSettings data)
		{
			mainWindow = main;
			Settings = data;

			ShortcutList = ShortcutManager.ListObservableShortcuts(data);
			AvailableThemes = App.Themes.GetAllAvailable();

			_selectedTheme = App.Themes.GetByFilename(Settings.Theme, out _) 
						  ?? App.Themes.GetByFilename("default.xml", out _)
						  ?? AvailableThemes.FirstOrDefault()
						  ?? App.Themes.GetFallback();
		}

		public void OnBeforeClose()
		{
			if (_isThemePreview) ThemeManager.Inst.ChangeTheme(_oldTheme);
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
			SettingsHelper.ApplyWindowState(mainWindow, Settings);
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

		private void UpdateThemePreview()
		{
			if (SelectedTheme == null || SelectedTheme.IsFallback) return;

			_isThemePreview = true;

			if (_oldTheme == null) _oldTheme = ThemeManager.Inst.CurrentTheme;
			ThemeManager.Inst.ChangeTheme(SelectedTheme);
		}
	}
}
