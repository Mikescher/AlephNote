using AlephNote.PluginInterface;
using System.Collections.Generic;
using AlephNote.Impl;
using MSHC.WPF.MVVM;

namespace AlephNote.WPF.Windows
{
	class AboutWindowViewmodel : ObservableObject
	{
		public string Appversion { get { return App.AppVersionProperty; } }

		public IEnumerable<IRemotePlugin> AvailableProvider { get { return PluginManager.Inst.LoadedPlugins; } }
	}
}
