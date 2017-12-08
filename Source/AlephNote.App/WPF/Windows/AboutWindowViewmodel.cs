using AlephNote.PluginInterface;
using System.Collections.Generic;
using AlephNote.Common.MVVM;
using AlephNote.Impl;

namespace AlephNote.WPF.Windows
{
	class AboutWindowViewmodel : ObservableObject
	{
		public string Appversion { get { return App.AppVersionProperty; } }

		public IEnumerable<IRemotePlugin> AvailableProvider { get { return PluginManager.Inst.LoadedPlugins; } }
	}
}
