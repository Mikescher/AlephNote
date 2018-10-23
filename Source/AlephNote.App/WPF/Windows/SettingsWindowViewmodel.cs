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
		public class CheckableAlephTheme : ObservableObject
		{
			public SettingsWindowViewmodel Owner;

			private AlephTheme _theme;
			public AlephTheme Theme { get { return _theme; } set { _theme = value; OnPropertyChanged(); } }

			private bool _checked;
			public bool Checked { get { return _checked; } set { _checked = value; OnPropertyChanged(); Owner.UpdateThemePreview(); } }
		}

		private readonly MainWindow mainWindow;

		public AppSettings Settings { get; private set; }

		public bool IsAppDebugMode => App.DebugMode;

		public ICommand InsertCurrentWindowStateCommand { get { return new RelayCommand(InsertCurrentWindowState); } }

		public ObservableCollectionNoReset<ObservableShortcutConfig> ShortcutList { get; set; }

		private ObservableShortcutConfig _selectedShortcut;
		public ObservableShortcutConfig SelectedShortcut { get { return _selectedShortcut; } set { _selectedShortcut = value; OnPropertyChanged(); } }
		
		public List<AlephTheme> AvailableThemes { get; set; }

		public List<CheckableAlephTheme> AvailableModifier { get; set; }

		private AlephTheme _selectedTheme;
		public AlephTheme SelectedTheme { get { return _selectedTheme; } set { _selectedTheme = value; OnPropertyChanged(); UpdateThemePreview(); } }
		
		private Visibility _hideAdvancedVisibility = Visibility.Visible;
		public Visibility HideAdvancedVisibility { get { return _hideAdvancedVisibility; } set { _hideAdvancedVisibility = value; OnPropertyChanged(); } }

		private AlephTheme _oldTheme;
		private List<AlephTheme> _oldModifiers;
		private bool _isThemePreview = false;

		public SettingsWindowViewmodel(MainWindow main, AppSettings data)
		{
			mainWindow = main;
			Settings = data;

			ShortcutList = ShortcutManager.ListObservableShortcuts(data);
			AvailableThemes = App.Themes.GetAllAvailableThemes();
			AvailableModifier = App.Themes.GetAllAvailableModifier().Select(m => new CheckableAlephTheme{Theme=m,Owner=this,Checked=data.ThemeModifier.Contains(m.SourceFilename)}).ToList();

			_selectedTheme = App.Themes.GetThemeByFilename(Settings.Theme, out _) 
						  ?? App.Themes.GetDefault()
						  ?? AvailableThemes.FirstOrDefault()
						  ?? App.Themes.GetFallback();

			_oldTheme = ThemeManager.Inst.CurrentBaseTheme;
			_oldModifiers = ThemeManager.Inst.CurrentModifers.ToList();
		}

		public void OnBeforeClose()
		{
			if (_isThemePreview) ThemeManager.Inst.ChangeTheme(_oldTheme, _oldModifiers);
		}

		public void OnBeforeApply()
		{
			var sdata = ShortcutList
				.Where(s => s.Key != AlephKey.None)
				.Select(s => Tuple.Create(s.Identifier, new ShortcutDefinition(s.Scope, s.Modifiers, s.Key)));

			Settings.Shortcuts = new KeyValueFlatCustomList<ShortcutDefinition>(sdata, ShortcutDefinition.DEFAULT);

			Settings.Theme = SelectedTheme.SourceFilename;
			Settings.ThemeModifier = new HashSet<string>(GetCheckedModifier().Select(p => p.SourceFilename));
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

		private IEnumerable<AlephTheme> GetCheckedModifier() => AvailableModifier.Where(p => p.Checked).Select(p => p.Theme);

		private void UpdateThemePreview()
		{
			if (SelectedTheme == null || SelectedTheme.ThemeType==AlephThemeType.Fallback) return;

			_isThemePreview = true;

			if (_oldTheme == null) _oldTheme = ThemeManager.Inst.CurrentBaseTheme;
			if (_oldModifiers == null) _oldModifiers = ThemeManager.Inst.CurrentModifers.ToList();
			ThemeManager.Inst.ChangeTheme(SelectedTheme, GetCheckedModifier());
		}
	}
}
