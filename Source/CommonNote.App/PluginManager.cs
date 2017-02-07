using CommonNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace CommonNote
{
	public static class PluginManager
	{
		private static List<ICommonNoteProvider> _provider = new List<ICommonNoteProvider>();

		public static void LoadPlugins()
		{
			var pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"plugins\");

			var pluginPaths = Directory.GetFiles(@".\plugins\", "*.dll");

			foreach (var path in pluginPaths)
			{
				try
				{
					LoadPlugin(path);
				}
				catch (ReflectionTypeLoadException e)
				{
					MessageBox.Show("Could not load plugin from " + path + "\r\n\r\n" + e + "\r\n\r\n" + string.Join("\r\n--------\r\n", e.LoaderExceptions.Select(p => p.ToString())), "Could not load plugin");
				}
				catch (Exception e)
				{
					MessageBox.Show("Could not load plugin from " + path + "\r\n\r\n" + e, "Could not load plugin");
				}
			}
		}

		private static void LoadPlugin(string path)
		{
			AssemblyName an = AssemblyName.GetAssemblyName(path);
			Assembly assembly = Assembly.Load(an);
			if (assembly == null) throw new Exception("Could not load assembly '" + an.FullName + "'");

			if (assembly != null)
			{
				Type[] types = assembly.GetTypes();
				foreach (Type type in types)
				{
					if (type.IsInterface || type.IsAbstract) continue;

					if (type.GetInterface(typeof(ICommonNoteProvider).FullName) != null)
					{
						ICommonNoteProvider instance = (ICommonNoteProvider)Activator.CreateInstance(type);

						if (instance == null) throw new Exception("Could not instantiate ICommonNotePlugin '" + type.FullName + "'");

						_provider.Add(instance);
					}
				}
			}
		}
	}
}
