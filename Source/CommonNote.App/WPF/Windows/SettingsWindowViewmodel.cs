using CommonNote.PluginInterface;
using CommonNote.Settings;
using System.Collections.Generic;
using MSHC.WPF.MVVM;

namespace CommonNote.WPF.Windows
{
	class SettingsWindowViewmodel : ObservableObject
	{
		public AppSettings Settings { get; private set; }

		public IEnumerable<IRemoteProvider> AvailableProvider { get { return PluginManager.LoadedPlugins; } }

		public SettingsWindowViewmodel(AppSettings data)
		{
			Settings = data;
		}
	}
}
