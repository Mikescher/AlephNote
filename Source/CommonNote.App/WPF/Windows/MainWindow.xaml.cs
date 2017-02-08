using CommonNote.Settings;
using System;
using System.IO;
using System.Windows;

namespace CommonNote.WPF.Windows
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			PluginManager.LoadPlugins();

			CommonNoteSettings settings;
			try
			{
				if (File.Exists(App.PATH_SETTINGS))
				{
					settings = CommonNoteSettings.Load();
				}
				else
				{
					settings = CommonNoteSettings.CreateEmpty();
					settings.Save();
				}
			}
			catch (Exception e)
			{
				MessageBox.Show("Could not load settings from " + App.PATH_SETTINGS + "\r\n\r\n" + e, "Could not load settings");
				settings = CommonNoteSettings.CreateEmpty();
			}

			DataContext = new MainWindowViewmodel(settings);
		}
	}
}
