using AlephNote.Common.MVVM;
using AlephNote.Common.Themes;
using AlephNote.Common.Util;
using AlephNote.WPF.Dialogs;
using AlephNote.WPF.MVVM;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace AlephNote.WPF.Windows
{
	public class ThemeEditorViewmodel : ObservableObject
	{
		public class ThemeEditorEntry : ObservableObject
		{
			private string _name = "";
			public string Name { get { return _name; } set { if (_name != value) { _name = value; OnPropertyChanged(); } } }

			private string _source = "";
			public string Source { get { return _source; } set { if (_source != value) { _source = value; OnPropertyChanged(); } } }

			private string _originalSource = "";
			public string OriginalSource { get { return _originalSource; } set { if (_originalSource != value) { _originalSource = value; OnPropertyChanged(); } } }

			private string _sourceFilename = "";
			public string SourceFilename { get { return _sourceFilename; } set { if (_sourceFilename != value) { _sourceFilename = value; OnPropertyChanged(); } } }

			public AlephTheme Parsed = null;
		}

		public class ThemeEditorDV : ObservableObject
		{
			private string _key = "";
			public string Key { get { return _key; } set { if (_key != value) { _key = value; OnPropertyChanged(); } } }

			private string _default = "";
			public string Default { get { return _default; } set { if (_default != value) { _default = value; OnPropertyChanged(); } } }

			private string _value = "";
			public string Value { get { return _value; } set { if (_value != value) { _value = value; OnPropertyChanged(); } } }

			private string _typestr = "";
			public string TypeStr { get { return _typestr; } set { if (_typestr != value) { _typestr = value; OnPropertyChanged(); } } }

			private bool _changed = false;
			public bool Changed { get { return _changed; } set { if (_changed != value) { _changed = value; OnPropertyChanged(); } } }
		}

		public ObservableCollection<ThemeEditorEntry> Entries { get; set; } = new ObservableCollection<ThemeEditorEntry>();
		public ObservableCollection<ThemeEditorDV> DefaultValues { get; set; } = new ObservableCollection<ThemeEditorDV>();
		
		private ThemeEditorEntry _selectedEntry = null;
		public ThemeEditorEntry SelectedEntry { get { return _selectedEntry; } set { if (_selectedEntry != value) { _selectedEntry = value; OnPropertyChanged(); PreviewCurrent(); } } }

		private string _errorText = "";
		public string ErrorText { get { return _errorText; } set { if (_errorText != value) { _errorText = value; OnPropertyChanged(); } } }

		public ICommand UndoCommand    { get { return new RelayCommand(UndoCurrent);    } }
		public ICommand SaveCommand    { get { return new RelayCommand(SaveCurrent);    } }
		public ICommand PreviewCommand { get { return new RelayCommand(PreviewCurrent); } }
		public ICommand NewCommand     { get { return new RelayCommand(NewTheme);       } }
		public ICommand ReloadCommand  { get { return new RelayCommand(ReloadCurrent);  } }

		private readonly ThemeEditor Owner;

		public ThemeEditorViewmodel(ThemeEditor owner)
		{
			Owner = owner;

			foreach (var at in ThemeManager.Inst.Cache.GetAllAvailable())
			{
				var newEntry = new ThemeEditorEntry()
				{
					SourceFilename = at.SourceFilename,
					Name = at.Name,
					Source = at.Source,
					OriginalSource = at.Source,
					Parsed = at,
				};
				Entries.Add(newEntry);
				if (at == ThemeManager.Inst.CurrentTheme) _selectedEntry = newEntry;
			}

			var def = ThemeManager.Inst.Cache.GetByFilename("default.xml", out _);

			foreach (var prop in ThemeManager.Inst.Cache.GetDefaultParserProperties())
			{
				DefaultValues.Add(new ThemeEditorDV()
				{
					Key = prop.Key,
					Default = prop.Value,
					TypeStr = prop.Value.GetType().Name,
					Value   = SelectedEntry?.Parsed?.GetStrRepr(prop.Key),
					Changed = SelectedEntry?.Parsed?.GetStrRepr(prop.Key) != def?.GetStrRepr(prop.Key)
				});
			}
		}

		private void UndoCurrent()
		{
			ErrorText = "";
			if (SelectedEntry == null) return;

			SelectedEntry.Source = SelectedEntry.OriginalSource;
		}

		private void SaveCurrent()
		{
			ErrorText = "";

			if (SelectedEntry == null) return;

			File.WriteAllText(Path.Combine(ThemeManager.Inst.Cache.BasePath, SelectedEntry.SourceFilename), SelectedEntry.Source, Encoding.UTF8);
			SelectedEntry.OriginalSource = SelectedEntry.Source;

			UpdateSelected();
		}

		private void ReloadCurrent()
		{
			ErrorText = "";
			if (SelectedEntry == null) return;

			SelectedEntry.OriginalSource = SelectedEntry.Source = File.ReadAllText(Path.Combine(ThemeManager.Inst.Cache.BasePath, SelectedEntry.SourceFilename));

			UpdateSelected();
		}

		private void PreviewCurrent()
		{
			ErrorText = "";
			if (SelectedEntry == null) return;

			try
			{
				var parser = new ThemeParser();
				parser.LoadFromString(SelectedEntry.Source, SelectedEntry.SourceFilename);
				parser.Parse(ThemeManager.Inst.Cache.GetDefaultParserProperties());
				var theme = parser.Generate();

				ThemeManager.Inst.Cache.ReplaceTheme(theme);

				ThemeManager.Inst.ChangeTheme(theme.SourceFilename);

				UpdateSelected();
			}
			catch (Exception e)
			{
				ErrorText = e.ToString();
			}
		}

		private void NewTheme()
		{
			ErrorText = "";
			try
			{
				if (!GenericInputDialog.ShowInputDialog(Owner, "Filename for new theme", "New Theme (filename)", "MyTheme.xml", out var filename)) throw new Exception("Aborted by user");

				if (!filename.ToLower().EndsWith(".xml")) throw new Exception("Filename must end with xml");
				if (Entries.Any(e => e.SourceFilename.ToLower() == filename.ToLower())) throw new Exception("Filename already exists");

				var newEntry = new ThemeEditorEntry()
				{
					OriginalSource = "",
					SourceFilename = filename,
					Name = "New Theme",
					Source = "",
				};

				Entries.Add(newEntry);
				SelectedEntry = newEntry;
			}
			catch (Exception e)
			{
				ErrorText = e.ToString();
			}
		}

		private void UpdateSelected()
		{
			if (SelectedEntry == null) return;
			try
			{
				var def = ThemeManager.Inst.Cache.GetByFilename("default.xml", out _);

				var parser = new ThemeParser();
				parser.LoadFromString(SelectedEntry.Source, SelectedEntry.SourceFilename);
				parser.Parse(ThemeManager.Inst.Cache.GetDefaultParserProperties());
				var theme = parser.Generate();

				SelectedEntry.Name = theme.Name;

				foreach (var dv in DefaultValues)
				{
					dv.Value   = theme.GetStrRepr(dv.Key);
					dv.Changed = theme.GetStrRepr(dv.Key) != def?.GetStrRepr(dv.Key);
				}
			}
			catch (Exception e)
			{
				ErrorText = e.ToString();
			}
		}
	}
}
