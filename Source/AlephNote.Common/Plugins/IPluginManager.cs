using System;
using System.Collections.Generic;
using AlephNote.Common.Network;
using AlephNote.PluginInterface;

namespace AlephNote.Common.Plugins
{
	public interface IPluginManager
	{
		IEnumerable<IRemotePlugin> LoadedPlugins { get; }

		void LoadPlugins(string baseDirectory, IAlephLogger logger);

		IRemotePlugin GetDefaultPlugin();
		IRemotePlugin GetPlugin(Guid uuid);

		IProxyFactory GetProxyFactory();
	}

	public static class PluginManagerSingleton
	{
		private static readonly object _lock = new object();

		public static IPluginManager Inst { get; private set; }

		public static void Register(IPluginManager man)
		{
			lock (_lock)
			{
				if (Inst != null) throw new NotSupportedException();

				Inst = man;
			}
		}

	}
}
