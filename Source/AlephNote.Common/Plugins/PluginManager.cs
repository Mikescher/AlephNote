using AlephNote.Common;
using AlephNote.PluginInterface;
using AlephNote.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace AlephNote.Plugins
{
	public static class PluginManager
	{
		private static HashSet<Guid> _pluginIDs = new HashSet<Guid>(); 
		private static List<IRemotePlugin> _provider = new List<IRemotePlugin>();
		private static IAlephLogger _logger;
		public static IEnumerable<IRemotePlugin> LoadedPlugins { get { return _provider; } }
		

		public static void LoadPlugins(string baseDirectory, IAlephLogger logger)
		{
			_provider = new List<IRemotePlugin>();
			_logger = logger;

			var pluginPath = Path.Combine(baseDirectory, @"plugins\");
			var pluginfiles = Directory.GetFiles(pluginPath, "*.dll");

			BasicRemoteConnection.SimpleJsonRestWrapper = (p, h) => new SimpleJsonRest(p, h, logger);

			foreach (var path in pluginfiles)
			{
				try
				{
					LoadPluginsFromAssembly(path);
				}
				catch (ReflectionTypeLoadException e)
				{
					logger.ShowExceptionDialog("Plugin load Error", "Could not load plugin from " + path, e, e.LoaderExceptions);
				}
				catch (Exception e)
				{
					logger.ShowExceptionDialog("Plugin load Error", "Could not load plugin from " + path, e);
				}
			}
		}

		private static void LoadPluginsFromAssembly(string path)
		{
			Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);

			if (assembly == null) throw new Exception("Could not load assembly '" + path + "'");

			Type[] types = assembly.GetTypes();
			foreach (Type type in types)
			{
				if (type.GetTypeInfo().IsInterface || type.GetTypeInfo().IsAbstract) continue;

				if (type.GetTypeInfo().GetInterface(typeof(IRemotePlugin).FullName) != null)
				{
					IRemotePlugin instance = (IRemotePlugin)Activator.CreateInstance(type);

					if (instance == null) throw new Exception("Could not instantiate IAlephNotePlugin '" + type.FullName + "'");

					instance.Init(_logger);

#if !DEBUG
					if (instance.GetVersion().Revision != 0)
					{
						App.Logger.Warn("PluginManager", string.Format("Ignore plugin {0}, debug version {1} ({2})", instance.DisplayTitleShort, instance.GetVersion(), instance.GetUniqueID()));
						continue;
					}
#endif

					if (_pluginIDs.Add(instance.GetUniqueID()))
					{
						_logger.Info("PluginManager", string.Format("Loaded plugin {0} in version {1} ({2})", instance.DisplayTitleShort, instance.GetVersion(), instance.GetUniqueID()));
						_provider.Add(instance);
					}
					else
					{
						_logger.Error("PluginManager", string.Format("Multiple plugins with the same ID ({0}) found", instance.GetUniqueID()));
					}
				}
			}
		}

		public static IRemotePlugin GetDefaultPlugin()
		{
			foreach (var plugin in LoadedPlugins)
			{
				if (plugin.GetUniqueID() == Guid.Parse("37de6de1-26b0-41f5-b252-5e625d9ecfa3")) return plugin; // Local Storage
			}

			return LoadedPlugins.First();
		}

		public static IRemotePlugin GetPlugin(Guid uuid)
		{
			return LoadedPlugins.FirstOrDefault(p => p.GetUniqueID() == uuid);
		}
	}
}
