using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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

	}
}
