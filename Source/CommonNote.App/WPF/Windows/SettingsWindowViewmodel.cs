using CommonNote.PluginInterface;
using CommonNote.Settings;
using MSHC.WPF.MVVM;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace CommonNote.WPF.Windows
{
	class SettingsWindowViewmodel : ObservableObject
	{
		private readonly MainWindow mainWindow;

		public AppSettings Settings { get; private set; }

		public IEnumerable<IRemoteProvider> AvailableProvider { get { return PluginManager.LoadedPlugins; } }

		public ICommand InsertCurrentWindowStateCommand { get { return new RelayCommand(InsertCurrentWindowState); } }

		public SettingsWindowViewmodel(MainWindow main, AppSettings data)
		{
			mainWindow = main;
			Settings = data;
		}

		private void InsertCurrentWindowState()
		{
			if (mainWindow.WindowState == WindowState.Maximized)
			{
				Settings.StartupLocation = WindowStartupLocation.CenterScreen;
				Settings.StartupState = WindowState.Maximized;
				Settings.StartupPositionX = (int)mainWindow.Left;
				Settings.StartupPositionY = (int)mainWindow.Top;
				Settings.StartupPositionWidth = (int)mainWindow.Width;
				Settings.StartupPositionHeight = (int)mainWindow.Height;
			}
			else if (mainWindow.WindowState == WindowState.Minimized)
			{
				Settings.StartupLocation = WindowStartupLocation.Manual;
				Settings.StartupState = WindowState.Minimized;
				Settings.StartupPositionX = (int)mainWindow.Left;
				Settings.StartupPositionY = (int)mainWindow.Top;
				Settings.StartupPositionWidth = (int)mainWindow.Width;
				Settings.StartupPositionHeight = (int)mainWindow.Height;
			}
			else if (mainWindow.WindowState == WindowState.Normal)
			{
				Settings.StartupLocation = WindowStartupLocation.Manual;
				Settings.StartupState = WindowState.Normal;
				Settings.StartupPositionX = (int)mainWindow.Left;
				Settings.StartupPositionY = (int)mainWindow.Top;
				Settings.StartupPositionWidth = (int)mainWindow.Width;
				Settings.StartupPositionHeight = (int)mainWindow.Height;
			}
			
		}
	}
}
