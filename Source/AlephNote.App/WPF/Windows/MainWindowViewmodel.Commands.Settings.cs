using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AlephNote.Common.Settings.Types;
using AlephNote.Common.Util;
using AlephNote.Properties;
using MSHC.WPF.MVVM;

namespace AlephNote.WPF.Windows
{
	public partial class MainWindowViewmodel
	{
		public ICommand SettingAlwaysOnTopCommand               => new RelayCommand(ChangeSettingAlwaysOnTop);
		public ICommand SettingLineNumbersCommand               => new RelayCommand(ChangeSettingLineNumbers);
		public ICommand SettingsWordWrapCommand                 => new RelayCommand(ChangeSettingWordWrap);
		public ICommand SettingsRotateThemeCommand              => new RelayCommand(ChangeSettingTheme);
		public ICommand SettingReadonlyModeCommand              => new RelayCommand(ChangeSettingReadonlyMode);
		
		public ICommand SetPreviewStyleSimpleCommand            => new RelayCommand(SetPreviewStyleSimple);
		public ICommand SetPreviewStyleExtendedCommand          => new RelayCommand(SetPreviewStyleExtended);
		public ICommand SetPreviewStyleSingleLinePreviewCommand => new RelayCommand(SetPreviewStyleSingleLinePreview);
		public ICommand SetPreviewStyleFullPreviewCommand       => new RelayCommand(SetPreviewStyleFullPreview);
		
		public ICommand SetNoteSortingNoneCommand               => new RelayCommand(SetNoteSortingNone);
		public ICommand SetNoteSortingByNameCommand             => new RelayCommand(SetNoteSortingByName);
		public ICommand SetNoteSortingByCreationDateCommand     => new RelayCommand(SetNoteSortingByCreationDate);
		public ICommand SetNoteSortingByModificationDateCommand => new RelayCommand(SetNoteSortingByModificationDate);

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
			var themes = ThemeManager.Inst.Cache.GetAllAvailableThemes();

			var idx = themes.IndexOf(ThemeManager.Inst.CurrentBaseTheme);
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
