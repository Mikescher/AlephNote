using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AlephNote.Common.Settings.Types;
using AlephNote.Common.Util;
using AlephNote.Properties;
using AlephNote.WPF.MVVM;

namespace AlephNote.WPF.Windows
{
	public partial class MainWindowViewmodel
	{
		public ICommand SettingAlwaysOnTopCommand  { get { return new RelayCommand(ChangeSettingAlwaysOnTop); } }
		public ICommand SettingLineNumbersCommand  { get { return new RelayCommand(ChangeSettingLineNumbers); } }
		public ICommand SettingsWordWrapCommand    { get { return new RelayCommand(ChangeSettingWordWrap); } }
		public ICommand SettingsRotateThemeCommand { get { return new RelayCommand(ChangeSettingTheme); } }
		public ICommand SettingReadonlyModeCommand { get { return new RelayCommand(ChangeSettingReadonlyMode); } }
		
		public ICommand SetPreviewStyleSimpleCommand            { get { return new RelayCommand(SetPreviewStyleSimple); } }
		public ICommand SetPreviewStyleExtendedCommand          { get { return new RelayCommand(SetPreviewStyleExtended); } }
		public ICommand SetPreviewStyleSingleLinePreviewCommand { get { return new RelayCommand(SetPreviewStyleSingleLinePreview); } }
		public ICommand SetPreviewStyleFullPreviewCommand       { get { return new RelayCommand(SetPreviewStyleFullPreview); } }
		
		public ICommand SetNoteSortingNoneCommand               { get { return new RelayCommand(SetNoteSortingNone); } }
		public ICommand SetNoteSortingByNameCommand             { get { return new RelayCommand(SetNoteSortingByName); } }
		public ICommand SetNoteSortingByCreationDateCommand     { get { return new RelayCommand(SetNoteSortingByCreationDate); } }
		public ICommand SetNoteSortingByModificationDateCommand { get { return new RelayCommand(SetNoteSortingByModificationDate); } }

		private void ChangeSettingAlwaysOnTop()
		{
			var ns = Settings.Clone();
			ns.AlwaysOnTop = !ns.AlwaysOnTop;
			ChangeSettings(ns);
		}
		
		private void ChangeSettingLineNumbers()
		{
			var ns = Settings.Clone();
			ns.SciLineNumbers = !ns.SciLineNumbers;
			ChangeSettings(ns);
		}

		private void ChangeSettingWordWrap()
		{
			var ns = Settings.Clone();
			ns.SciWordWrap = !ns.SciWordWrap;
			ChangeSettings(ns);
		}

		public void ChangeSettingReadonlyMode()
		{
			var ns = Settings.Clone();
			ns.IsReadOnlyMode = !ns.IsReadOnlyMode;
			ChangeSettings(ns);
		}

		private void ChangeSettingTheme()
		{
			var themes = ThemeManager.Inst.Cache.GetAllAvailable();

			var idx = themes.IndexOf(ThemeManager.Inst.CurrentTheme);
			if (idx < 0) return;

			idx = (idx + 1) % themes.Count;

			var ns = Settings.Clone();
			ns.Theme = themes[idx].SourceFilename;
			ChangeSettings(ns);
		}

		private void SetPreviewStyleSimple()
		{
			var ns = Settings.Clone();
			ns.NotePreviewStyle = NotePreviewStyle.Simple;
			ChangeSettings(ns);
		}
		
		private void SetPreviewStyleExtended()
		{
			var ns = Settings.Clone();
			ns.NotePreviewStyle = NotePreviewStyle.Extended;
			ChangeSettings(ns);
		}
		
		private void SetPreviewStyleSingleLinePreview()
		{
			var ns = Settings.Clone();
			ns.NotePreviewStyle = NotePreviewStyle.SingleLinePreview;
			ChangeSettings(ns);
		}
		
		private void SetPreviewStyleFullPreview()
		{
			var ns = Settings.Clone();
			ns.NotePreviewStyle = NotePreviewStyle.FullPreview;
			ChangeSettings(ns);
		}

		private void SetNoteSortingNone()
		{
			var ns = Settings.Clone();
			ns.NoteSorting = SortingMode.None;
			ChangeSettings(ns);
		}

		private void SetNoteSortingByName()
		{
			var ns = Settings.Clone();
			ns.NoteSorting = SortingMode.ByName;
			ChangeSettings(ns);
		}

		private void SetNoteSortingByCreationDate()
		{
			var ns = Settings.Clone();
			ns.NoteSorting = SortingMode.ByCreationDate;
			ChangeSettings(ns);
		}

		private void SetNoteSortingByModificationDate()
		{
			var ns = Settings.Clone();
			ns.NoteSorting = SortingMode.ByModificationDate;
			ChangeSettings(ns);
		}
	}
}
