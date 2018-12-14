using AlephNote.PluginInterface;
using System.Windows;
using System.Windows.Input;
using System;
using System.Linq;
using AlephNote.Common.Settings;
using AlephNote.Common.Settings.Types;
using AlephNote.WPF.Shortcuts;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AlephNote.Common.SPSParser;
using AlephNote.Common.Themes;
using AlephNote.Common.Util;
using AlephNote.WPF.Util;
using MSHC.WPF.MVVM;
using RelayCommand = AlephNote.WPF.MVVM.RelayCommand;

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
		
		public class EditableSnippet : ObservableObject
		{
			public Func<string, string> PreviewFunc;

			private string _id = "";
			public string ID { get { return _id; } set { _id = value; OnPropertyChanged(); } }

			private string _name = "";
			public string Name { get { return _name; } set { _name = value; OnPropertyChanged(); } }

			private string _value = "";
			public string Value { get { return _value; } set { _value = value; OnPropertyChanged(); Update(); } }

			private string _preview = "";
			public string Preview { get { return _preview; } set { _preview = value; OnPropertyChanged(); } }

			public void Update() { Preview = PreviewFunc(_value); }
		}
		
		private readonly SimpleParamStringParser _spsParser = new SimpleParamStringParser();

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

		public ObservableCollectionNoReset<EditableSnippet> SnippetList { get; set; }

		private EditableSnippet _selectedSnippet = null;
		public EditableSnippet SelectedSnippet { get { return _selectedSnippet; } set { _selectedSnippet = value; OnPropertyChanged(); value?.Update(); } }
		
		private string _newSnippetID = "";
		public string NewSnippetID { get { return _newSnippetID; } set { _newSnippetID = value; OnPropertyChanged(); } }

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

			SnippetList = new ObservableCollectionNoReset<EditableSnippet>(data.Snippets.Data.Select(p => new EditableSnippet{PreviewFunc=SnippetPrev,ID=p.Key,Name=p.Value.DisplayName,Value=p.Value.Value}));
			NewSnippetID = GetNextSnippetID();
		}

		public void OnBeforeClose()
		{
			if (_isThemePreview) ThemeManager.Inst.ChangeTheme(_oldTheme, _oldModifiers);
		}

		public void OnBeforeApply()
		{
			var sdata = ShortcutList
				.Where(s => s.Key != AlephKey.None)
				//.Where(s => !s.Identifier.StartsWith("Snippet::") || SnippetList.Any(sl => sl.ID == s.Identifier.Substring("Snippet::".Length)))
				.Select(s => Tuple.Create(s.Identifier, new ShortcutDefinition(s.Scope, s.Modifiers, s.Key)))
				.ToList();

			Settings.Shortcuts = new KeyValueFlatCustomList<ShortcutDefinition>(sdata, ShortcutDefinition.DEFAULT);

			Settings.Theme = SelectedTheme.SourceFilename;
			Settings.ThemeModifier = new HashSet<string>(GetCheckedModifier().Select(p => p.SourceFilename));

			Settings.Snippets = new KeyValueCustomList<SnippetDefinition>(SnippetList.Select(p => Tuple.Create(p.ID, new SnippetDefinition(p.Name, p.Value))), SnippetDefinition.DEFAULT);
		}

		private string SnippetPrev(string v)
		{
			var p = _spsParser.Parse(v, mainWindow.VM.Repository, mainWindow.VM.SelectedNote, out var succ);
			return succ ? p : v;
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
		
		public void AddSnippet()
		{
			if (string.IsNullOrWhiteSpace(NewSnippetID)) { MessageBox.Show("Please insert a valid ID.", "Not ID supplied", MessageBoxButton.OK, MessageBoxImage.Exclamation); return; }
			if (!Regex.IsMatch(NewSnippetID, @"^[0-9A-Za-z_+;,\.~=()\(\)]+$")) { MessageBox.Show("Please insert a valid ID.", "Invalid ID", MessageBoxButton.OK, MessageBoxImage.Exclamation); return; }
			if (SnippetList.Any(s => s.ID.ToLower() == NewSnippetID.ToLower())) { MessageBox.Show("The ID already exists.", "Duplicate ID", MessageBoxButton.OK, MessageBoxImage.Exclamation); return; }

			var snp = new EditableSnippet{ PreviewFunc = SnippetPrev, ID = NewSnippetID, Name = "Custom", Preview = "", Value = "{now}" };
			SnippetList.Add(snp);

			SelectedSnippet = snp;

			NewSnippetID = GetNextSnippetID();
		}

		private string GetNextSnippetID()
		{
			for (int i = 1;; i++)
			{
				var id = "custom_"+i;
				if (SnippetList.Any(s => s.ID.ToLower() == id)) continue;
				return id;
			}
		}

		public void RemoveSnippet()
		{
			if (SelectedSnippet == null) return;
			SnippetList.Remove(SelectedSnippet);
			SelectedSnippet = null;
		}
	}
}
