using AlephNote.PluginInterface;
using AlephNote.Plugins;
using AlephNote.Settings;
using AlephNote.WPF.MVVM;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System;
using System.Linq;
using AlephNote.Common.Settings.Types;

namespace AlephNote.WPF.Windows
{
	class SettingsWindowViewmodel : ObservableObject
	{
		private readonly MainWindow mainWindow;

		public AppSettings Settings { get; private set; }
		
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
				Settings.StartupLocation = ExtendedWindowStartupLocation.CenterScreen;
				Settings.StartupState = ExtendedWindowState.Maximized;
				Settings.StartupPositionX = (int)mainWindow.Left;
				Settings.StartupPositionY = (int)mainWindow.Top;
				Settings.StartupPositionWidth = (int)mainWindow.Width;
				Settings.StartupPositionHeight = (int)mainWindow.Height;
			}
			else if (mainWindow.WindowState == WindowState.Minimized)
			{
				Settings.StartupLocation = ExtendedWindowStartupLocation.Manual;
				Settings.StartupState = ExtendedWindowState.Minimized;
				Settings.StartupPositionX = (int)mainWindow.Left;
				Settings.StartupPositionY = (int)mainWindow.Top;
				Settings.StartupPositionWidth = (int)mainWindow.Width;
				Settings.StartupPositionHeight = (int)mainWindow.Height;
			}
			else if (mainWindow.WindowState == WindowState.Normal)
			{
				Settings.StartupLocation = ExtendedWindowStartupLocation.Manual;
				Settings.StartupState = ExtendedWindowState.Normal;
				Settings.StartupPositionX = (int)mainWindow.Left;
				Settings.StartupPositionY = (int)mainWindow.Top;
				Settings.StartupPositionWidth = (int)mainWindow.Width;
				Settings.StartupPositionHeight = (int)mainWindow.Height;
			}

		}

		public void AddAccount(IRemotePlugin p)
		{
			var acc = new RemoteStorageAccount(Guid.NewGuid(), p, p.CreateEmptyRemoteStorageConfiguration());

			Settings.AddAccountAndSetActive(acc);


		}

		public void RemoveAccount()
		{
			if (Settings.Accounts.Count <= 1) return;

			Settings.RemoveAccount(Settings.ActiveAccount);
		}
	}
}
