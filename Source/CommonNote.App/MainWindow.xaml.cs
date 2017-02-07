using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using CommonNote.PluginInterface;

namespace CommonNote
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			LoadPlugins();
		}

		private void LoadPlugins()
		{
			var plugins = Directory.GetFiles(@".\plugins\", "*.dll");

			foreach (var plugin in plugins)
			{
				try
				{
					AssemblyName an = AssemblyName.GetAssemblyName(plugin);
					Assembly assembly = Assembly.Load(an);

					Type pluginType = typeof(ICommonNotePlugin);

					if (assembly != null)
					{
						Type[] types = assembly.GetTypes();
						foreach (Type type in types)
						{
							if (type.IsInterface || type.IsAbstract)
							{
								continue;
							}
							else
							{
								if (type.GetInterface(pluginType.FullName) != null)
								{
									ICommonNotePlugin pluginInst = (ICommonNotePlugin)Activator.CreateInstance(type);
									MessageBox.Show(this, pluginInst.GetName() + " :: " + pluginInst.GetVersion());
								}
							}
						}
					}
				}
				catch (Exception e)
				{
					MessageBox.Show(this, e.ToString());
				}
				MessageBox.Show(this, plugin);
			}
		}
	}
}
