using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AlephNote.Common.Operations;
using AlephNote.Common.Repository;
using AlephNote.Common.Settings;
using AlephNote.PluginInterface.Util;
using AlephNote.WPF.Dialogs;
using MSHC.WPF.MVVM;
using AlephNote.WPF.Util;
using Microsoft.Win32;
using MSHC.Lang.Collections;

namespace AlephNote.WPF.Windows
{
	public partial class MainWindowViewmodel
	{
		public ICommand SettingsCommand                => new RelayCommand(ShowSettings);
		public ICommand ResyncCommand                  => new RelayCommand(Resync);
		public ICommand ShowMainWindowCommand          => new RelayCommand(ShowMainWindow);
		public ICommand ExitCommand                    => new RelayCommand(Exit);
		public ICommand RestartCommand                 => new RelayCommand(Restart);
		public ICommand ShowAboutCommand               => new RelayCommand(ShowAbout);
		public ICommand ShowLogCommand                 => new RelayCommand(ShowLog);
		public ICommand SaveAndSyncCommand             => new RelayCommand(SaveAndSync);
		public ICommand FullResyncCommand              => new RelayCommand(FullResync);
		public ICommand ManuallyCheckForUpdatesCommand => new RelayCommand(ManuallyCheckForUpdates);
		public ICommand HideCommand                    => new RelayCommand(HideMainWindow);
		public ICommand FocusScintillaCommand          => new RelayCommand(() => Owner.FocusScintilla());
		public ICommand FocusNotesListCommand          => new RelayCommand(() => Owner.NotesViewControl.FocusNotesList());
		public ICommand FocusGlobalSearchCommand       => new RelayCommand(() => Owner.FocusGlobalSearch());
		public ICommand FocusFolderCommand             => new RelayCommand(() => Owner.NotesViewControl.FocusFolderList());
		public ICommand ShowTagFilterCommand           => new RelayCommand(ShowTagFilter);
		public ICommand AppToggleVisibilityCommand     => new RelayCommand(AppToggleVisibility);
		
		public ICommand FocusNextDirectoryAnyCommand   => new RelayCommand(FocusNextDirectoryAny);
		public ICommand FocusPrevDirectoryAnyCommand   => new RelayCommand(FocusPrevDirectoryAny);
		public ICommand FocusNextNoteCommand           => new RelayCommand(FocusNextNote);
		public ICommand FocusPrevNoteCommand           => new RelayCommand(FocusPrevNote);
		public ICommand FocusNextDirectoryCommand      => new RelayCommand(FocusNextDirectory);
		public ICommand FocusPrevDirectoryCommand      => new RelayCommand(FocusPrevDirectory);
		public ICommand FocusDirectoryLevelDownCommand => new RelayCommand(FocusDirectoryLevelDown);
		public ICommand FocusDirectoryLevelUpCommand   => new RelayCommand(FocusDirectoryLevelUp);

		public ICommand RotateNoteProviderCommand => new RelayCommand(RotateNoteProvider);
		
		public void ShowMainWindow()
		{
			Owner.Show();
			if (WindowState == WindowState.Minimized) WindowState = WindowState.Normal;
			Owner.Activate();
			Owner.Focus();
			Owner.FocusScintillaDelayed(150);
		}
		
		private void HideMainWindow()
		{
			Owner.Hide();
		}

		private void AppToggleVisibility()
		{
			if (Owner.IsVisible)
				HideMainWindow();
			else
				ShowMainWindow();
		}

		private void ManuallyCheckForUpdates()
		{
			try
			{
				var ghc = new GithubConnection();
				var r = ghc.GetLatestRelease(Settings.UpdateToPrerelease);

				if (r.Item1 > App.APP_VERSION)
				{
					UpdateWindow.Show(Owner, this, r.Item1, r.Item2, r.Item3);
				}
				else
				{
					MessageBox.Show("You are using the latest version");
				}
			}
			catch (Exception e)
			{
				App.Logger.Error("Main", "Can't get latest version from github", e);
				MessageBox.Show("Cannot get latest version from github API");
			}
		}

		public void Exit()
		{
			_forceClose = true;
			Owner.Close();
		}

		private void Restart()
		{
			_forceClose = true;
			Owner.Close();

			ProcessStartInfo info = new ProcessStartInfo
			{
				Arguments = "/C ping 127.0.0.1 -n 3 && \"" + Environment.GetCommandLineArgs()[0] + "\"",
				WindowStyle = ProcessWindowStyle.Hidden,
				CreateNoWindow = true,
				FileName = "cmd.exe"
			};
			Process.Start(info);
		}

		private void ShowAbout()
		{
			new AboutWindow{ Owner = Owner }.ShowDialog();
		}

		private void ShowLog()
		{
			new LogWindow { Owner = Owner }.Show();
		}

		private void SaveAndSync()
		{
			try
			{
				Repository.SaveAll();
				Repository.SyncNow();
			}
			catch (Exception e)
			{
				App.Logger.Error("Main", "Synchronization failed", e);
				ExceptionDialog.Show(Owner, "Synchronization failed", e, string.Empty);
			}
		}

		private void FullResync()
		{
			if (Repository.ProviderUID == Guid.Parse("37de6de1-26b0-41f5-b252-5e625d9ecfa3")) return; // no full resync in headless

			try
			{
				if (MessageBox.Show(Owner, "Do you really want to delete all local data and download the server data?", "Full resync?", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

				Repository.Shutdown(false);

				Repository.DeleteLocalData();

				_repository = new NoteRepository(AppSettings.PATH_LOCALDB, this, Settings, Settings.ActiveAccount, dispatcher);
				_repository.Init();

				OnExplicitPropertyChanged("Repository");

				SelectedNote = null;
			}
			catch (Exception e)
			{
				App.Logger.Error("Main", "Full Synchronization failed", e);
				ExceptionDialog.Show(Owner, "Full Synchronization failed", e, string.Empty);
			}
		}
		
		private void ShowSettings()
		{
			var registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

			Settings.LaunchOnBoot = registryKey?.GetValue(string.Format(AppSettings.APPNAME_REG, Settings.ClientID)) != null;

			new SettingsWindow(this, Settings) {Owner = Owner}.ShowDialog();
		}
		
		private void Resync()
		{
			Repository.SyncNow();
		}
		
		private void ShowTagFilter()
		{
			Owner.ShowTagFilter();
		}

		private void FocusPrevNote()
		{
			var vnotes = Owner.NotesViewControl.EnumerateVisibleNotes().ToList();
			var idx = vnotes.IndexOf(SelectedNote);
			if (idx == -1) return;
			
			idx--;

			if (idx >= 0 && idx < vnotes.Count) SelectedNote = vnotes[idx];
		}
		
		private void FocusNextNote()
		{
			var vnotes = Owner.NotesViewControl.EnumerateVisibleNotes().ToList();
			var idx = vnotes.IndexOf(SelectedNote);
			if (idx == -1) return;
			
			idx++;

			if (idx >= 0 && idx < vnotes.Count) SelectedNote = vnotes[idx];
		}

		private void FocusPrevDirectoryAny()
		{
			if (SelectedFolderPath == null) return;
			
			var folders = Owner.NotesViewControl.ListFolder().ToList();
			
			var idx = folders.IndexOf(SelectedFolderPath);
			if (idx == -1) return;

			idx--;
			
			if (idx >= 0 && idx < folders.Count) SelectedFolderPath = folders[idx];
		}

		private void FocusNextDirectoryAny()
		{
			if (SelectedFolderPath == null) return;
			
			var folders = Owner.NotesViewControl.ListFolder().ToList();
			
			var idx = folders.IndexOf(SelectedFolderPath);
			if (idx == -1) return;

			idx++;
			
			if (idx >= 0 && idx < folders.Count) SelectedFolderPath = folders[idx];
		}

		private void FocusPrevDirectory()
		{
			if (SelectedFolderPath == null) return;

			var folders = Owner.NotesViewControl.ListFolder().ToList();
			
			var idx = folders.IndexOf(SelectedFolderPath);
			if (idx == -1) return;

			folders = folders.Take(idx).Reverse().Concat(folders.Skip(idx+1).Reverse()).ToList();
			
			if (HierachicalBaseWrapper.IsSpecialOrRoot(SelectedFolderPath))
			{
				var found = folders.FirstOrDefault(HierachicalBaseWrapper.IsSpecialOrRoot);
				if (found != null) SelectedFolderPath = found;
			}
			else
			{
				var found = folders.FirstOrDefault(p => !HierachicalBaseWrapper.IsSpecialOrRoot(p) && p.IsDirectSubPathOf(SelectedFolderPath.ParentPath(), true));
				if (found != null) SelectedFolderPath = found;
			}
		}

		private void FocusNextDirectory()
		{
			if (SelectedFolderPath == null) return;

			var folders = Owner.NotesViewControl.ListFolder().ToList();
			
			var idx = folders.IndexOf(SelectedFolderPath);
			if (idx == -1) return;

			folders = folders.Skip(idx+1).Concat(folders.Take(idx)).ToList();

			if (HierachicalBaseWrapper.IsSpecialOrRoot(SelectedFolderPath))
			{
				var found = folders.FirstOrDefault(HierachicalBaseWrapper.IsSpecialOrRoot);
				if (found != null) SelectedFolderPath = found;
			}
			else
			{
				var found = folders.FirstOrDefault(p => !HierachicalBaseWrapper.IsSpecialOrRoot(p) && p.IsDirectSubPathOf(SelectedFolderPath.ParentPath(), true));
				if (found != null) SelectedFolderPath = found;
			}
		}

		private void FocusDirectoryLevelDown()
		{
			if (SelectedFolderPath == null) return;
			
			var folders = Owner.NotesViewControl.ListFolder().ToList();

			if (HierachicalBaseWrapper.IsSpecial(SelectedFolderPath))
			{
				var found = folders.FirstOrDefault(p => !HierachicalBaseWrapper.IsSpecial(p));
				if (found != null) SelectedFolderPath = found;
				return;
			}
			else
			{
				var found = folders.FirstOrDefault(p => p.IsDirectSubPathOf(SelectedFolderPath, true));
				if (found != null) SelectedFolderPath = found;
			}
		}

		private void FocusDirectoryLevelUp()
		{
			if (SelectedFolderPath == null) return;
			if (SelectedFolderPath.IsRoot()) return;
			if (HierachicalBaseWrapper.IsSpecial(SelectedFolderPath)) return;

			SelectedFolderPath = SelectedFolderPath.ParentPath();
		}

		private void RotateNoteProvider()
		{
			if (Settings.Accounts.Count <= 1) return;

			var accounts = Settings.Accounts.ToList();
			var idx = accounts.FirstOrDefaultIndex(a => a.IsEqual(Settings.ActiveAccount)) ?? -1;
			if (idx == -1) return;

			idx = (idx+1) % accounts.Count;

			ChangeAccount(accounts[idx].ID);
		}
	}
}
