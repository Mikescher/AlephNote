using AlephNote.PluginInterface;
using System.Collections.Generic;
using AlephNote.Impl;
using MSHC;
using MSHC.WPF.MVVM;

namespace AlephNote.WPF.Windows
{
	class AboutWindowViewmodel : ObservableObject
	{
		public string Appversion => App.AppVersionProperty;

		public string LanguageUtilsVersion => LanguageUtils.VERSION.Revision==0 ? LanguageUtils.VERSION.ToString(4) : LanguageUtils.VERSION.ToString(3);
		public string WPFUtilsVersion      => WPFUtils.VERSION.Revision==0      ? WPFUtils.VERSION.ToString(4)      : WPFUtils.VERSION.ToString(3);

		public IEnumerable<IRemotePlugin> AvailableProvider { get { return PluginManager.Inst.LoadedPlugins; } }
	}
}
