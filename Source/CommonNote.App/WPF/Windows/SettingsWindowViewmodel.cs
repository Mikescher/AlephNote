using CommonNote.PluginInterface;
using CommonNote.Settings;
using MSHC.MVVM;
using System.Collections.Generic;

namespace CommonNote.WPF.Windows
{
	class SettingsWindowViewmodel : ObservableObject
	{
		public CommonNoteSettings Settings { get; private set; }

		public IEnumerable<ICommonNoteProvider> AvailableProvider { get { return PluginManager.LoadedPlugins; } }

		public SettingsWindowViewmodel(CommonNoteSettings data)
		{
			Settings = data;
		}
	}
}
