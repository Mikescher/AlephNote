using System;
using System.Collections.Generic;
using AlephNote.Common.Network;
using AlephNote.PluginInterface;

namespace AlephNote.Common.Plugins
{
	public interface IPluginManager
	{
		IEnumerable<IRemotePlugin> LoadedPlugins { get; }

		void LoadPlugins(string baseDirectory);

		IRemotePlugin GetDefaultPlugin();
		IRemotePlugin GetPlugin(Guid uuid);

		IProxyFactory GetProxyFactory();
	}
}
