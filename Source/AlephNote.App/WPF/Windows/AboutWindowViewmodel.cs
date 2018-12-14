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

		public string CSUtilsVersion => CSharpUtils.VERSION.Revision==0 ? CSharpUtils.VERSION.ToString(4) : CSharpUtils.VERSION.ToString(3);

		public IEnumerable<IRemotePlugin> AvailableProvider { get { return PluginManager.Inst.LoadedPlugins; } }
	}
}
