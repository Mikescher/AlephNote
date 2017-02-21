using AlephNote.PluginInterface;
using AlephNote.WPF.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AlephNote.Plugins
{
	public static class PluginManager
	{
		private static List<IRemotePlugin> _provider = new List<IRemotePlugin>();
		public static IEnumerable<IRemotePlugin> LoadedPlugins { get { return _provider; } }

		public static void LoadPlugins()
		{
			_provider = new List<IRemotePlugin>();

			var pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"plugins\");
			var pluginfiles = Directory.GetFiles(pluginPath, "*.dll");

			foreach (var path in pluginfiles)
			{
				try
				{
					LoadPlugin(path);
				}
				catch (ReflectionTypeLoadException e)
				{
					ExceptionDialog.Show(null, "Plugin load Error", "Could not load plugin from " + path, e, e.LoaderExceptions);
				}
				catch (Exception e)
				{
					ExceptionDialog.Show(null, "Plugin load Error", "Could not load plugin from " + path, e);
				}
			}
		}

		private static void LoadPlugin(string path)
		{
			AssemblyName an = AssemblyName.GetAssemblyName(path);
			Assembly assembly = Assembly.Load(an);

			if (assembly == null) throw new Exception("Could not load assembly '" + an.FullName + "'");

			Type[] types = assembly.GetTypes();
			foreach (Type type in types)
			{
				if (type.IsInterface || type.IsAbstract) continue;

				if (type.GetInterface(typeof(IRemotePlugin).FullName) != null)
				{
					IRemotePlugin instance = (IRemotePlugin)Activator.CreateInstance(type);

					if (instance == null) throw new Exception("Could not instantiate IAlephNotePlugin '" + type.FullName + "'");

					_provider.Add(instance);
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
