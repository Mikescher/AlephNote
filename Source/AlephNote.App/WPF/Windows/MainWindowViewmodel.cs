using AlephNote.Common.Repository;
using AlephNote.Common.Settings.Types;
using AlephNote.Common.SPSParser;
using AlephNote.PluginInterface;
using AlephNote.WPF.Shortcuts;
using AlephNote.WPF.Util;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using AlephNote.Common.Operations;
using AlephNote.Common.Settings;
using AlephNote.Impl;
using AlephNote.PluginInterface.Util;
using AlephNote.Common.Util;
using AlephNote.PluginInterface.Exceptions;
using AlephNote.WPF.Dialogs;
using MSHC.Lang.Collections;
using MSHC.Lang.Special;
using MSHC.Util.Threads;
using MSHC.WPF.MVVM;

namespace AlephNote.WPF.Windows
{
	public partial class MainWindowViewmodel : ObservableObject, ISynchronizationFeedback, IThemeListener
	{
		public ICommand ClosingEvent { get { return new RelayCommand<CancelEventArgs>(OnClosing); } }
		public ICommand CloseEvent { get { return new RelayCommand<EventArgs>(OnClose); } }
		public ICommand StateChangedEvent { get { return new RelayCommand<EventArgs>(OnStateChanged); } }

		private AppSettings _settings;
		public AppSettings Settings { get { return _settings; } private set { _settings = value; OnPropertyChanged(); SettingsChanged(); } }

		private NoteRepository _repository;
		public NoteRepository Repository { get { return _repository; } private set { _repository = value; OnPropertyChanged(); } }

		private INote _lastSelectedNote = null;
		private INote _selectedNote;
		public INote SelectedNote { get { return _selectedNote; } set { if (_selectedNote != value) { _selectedNote = value; OnPropertyChanged(); SelectedNoteChanged(); } } }

		private DirectoryPath _selectedFolderPath;
		public DirectoryPath SelectedFolderPath { get { return _selectedFolderPath; } set { if (_selectedFolderPath != value) { _selectedFolderPath = value; OnPropertyChanged(); SelectedFolderPathChanged(); } } }

		private DateTimeOffset? _lastSynchronized = null;

		private string _lastSynchronizedText = "never";
		public string LastSynchronizedText { get { return _lastSynchronizedText; } set { _lastSynchronizedText = value; OnPropertyChanged(); } }

		private string _searchText = string.Empty;
		public string SearchText { get { return _searchText; } set { if (_searchText != value) { _searchText = value; OnPropertyChanged(); FilterNoteList();} } }

		private WindowState _windowState = WindowState.Normal;
		public WindowState WindowState { get { return _windowState; } set { _windowState = value; OnPropertyChanged(); } }

		private SynchronizationState _synchronizationState = SynchronizationState.UpToDate;
		public SynchronizationState SynchronizationState { get { return _synchronizationState; } set { if (value != _synchronizationState) { _synchronizationState = value; OnPropertyChanged(); } } }

		public bool DebugMode { get { return App.DebugMode; } }

		private GridLength _overviewGridLength = new GridLength(0);
		public GridLength OverviewListWidth
		{
			get { return _overviewGridLength; }
			set
			{
				if (value != _overviewGridLength)
				{
					_overviewGridLength = value;
					OnPropertyChanged();
					Settings.OverviewListWidth = value.Value;
					GridSplitterChanged();
				}
			}
		}

		public string FullVersion { get { return "AlephNote v" + App.APP_VERSION; } }

		bool IThemeListener.IsTargetAlive => true;

		private readonly SynchronizationDispatcher dispatcher = new SynchronizationDispatcher();
		private readonly DelayedCombiningInvoker _invSaveSettings;
		private readonly SimpleParamStringParser _spsParser = new SimpleParamStringParser();
		private readonly ScrollCache _scrollCache;

		public readonly StackingBool PreventScintillaFocusLock = new StackingBool();
		private bool _forceClose = false;

		public readonly MainWindow Owner;
		
		public MainWindowViewmodel(AppSettings settings, MainWindow parent)
		{
			Owner = parent;

			_settings = settings;
			_invSaveSettings = DelayedCombiningInvoker.Create(() => Application.Current.Dispatcher.BeginInvoke(new Action(SaveSettings)), 8 * 1000, 60 * 1000);

			_repository = new NoteRepository(AppSettings.PATH_LOCALDB, this, settings, settings.ActiveAccount, dispatcher);
			Repository.Init();

			_scrollCache = Settings.RememberScroll ? ScrollCache.LoadFromFile(AppSettings.PATH_SCROLLCACHE) : ScrollCache.CreateEmpty(AppSettings.PATH_SCROLLCACHE);

			Owner.TrayIcon.Visibility = (Settings.CloseToTray || Settings.MinimizeToTray) ? Visibility.Visible : Visibility.Collapsed;


			var initialNote   = _settings.LastSelectedNote;
			var initialFolder = _settings.LastSelectedFolder;
			if (initialNote != null && _settings.RememberLastSelectedNote)
			{
				if (initialFolder != null) SelectedFolderPath = initialFolder;
				SelectedNote = Repository.Notes.FirstOrDefault(n => n.UniqueName== initialNote);
			}
			if (SelectedNote == null) SelectedNote = Repository.Notes.FirstOrDefault();


			OverviewListWidth = new GridLength(settings.OverviewListWidth);

			if (settings.CheckForUpdates)
			{
				var t = new Thread(CheckForUpdatesAsync) { Name = "UPDATE_CHECK" };
				t.Start();
			}

#if !DEBUG
			if (settings.SendAnonStatistics)
			{
				var t = new Thread(UploadUsageStatsAsync) { Name = "STATISTICS_UPLOAD" };
				t.Start();
			}
#endif

			SettingsChanged();

			ThemeManager.Inst.RegisterSlave(this);
		}

		public void ChangeSettings(AppSettings newSettings)
		{
			try
			{
				var sw = Stopwatch.StartNew();

				// disable LocalSync when changing account
				if (!Settings.ActiveAccount.IsEqual(newSettings.ActiveAccount) && !newSettings.RawFolderRepoSubfolders) newSettings.UseRawFolderRepo = false;

				var diffs = AppSettings.Diff(Settings, newSettings);

				var reconnectRepo            = diffs.Any(d => d.Attribute.ReconnectRepo) || (!Settings.ActiveAccount.IsEqual(newSettings.ActiveAccount));
				var refreshNotesViewTemplate = diffs.Any(d => d.Attribute.RefreshNotesViewTemplate);
				var refreshNotesCtrlView     = diffs.Any(d => d.Attribute.RefreshNotesControlView);
				var refreshNotesTheme        = diffs.Any(d => d.Attribute.RefreshNotesTheme);

				App.Logger.Debug("Main", 
					$"Settings changed by user ({diffs.Count} diffs)", 
					$"reconnectRepo: {reconnectRepo}\n" +  
					$"refreshNotesViewTemplate: {refreshNotesViewTemplate}\n" +  
					$"refreshNotesCtrlView: {refreshNotesCtrlView}\n" +  
					$"refreshNotesTheme: {refreshNotesTheme}\n" +  
					"\n\nDifferences:\n" +  
					string.Join("\n", diffs.Select(d => " - " + d.PropInfo.Name)));

				if (reconnectRepo)
				{
					try
					{
						_repository.Shutdown();
					}
					catch (Exception e)
					{
						App.Logger.Error("Main", "Shutting down current connection failed", e);
						ExceptionDialog.Show(Owner, "Shutting down current connection failed.\r\nConnection will be forcefully aborted", e, string.Empty);
						_repository.KillThread();
					}
				}

				var sw2 = Stopwatch.StartNew();
				Settings = newSettings;
				Settings.Save();
				App.Logger.Trace("Main", $"Settings saved in {sw2.ElapsedMilliseconds}ms");

				if (refreshNotesTheme) ThemeManager.Inst.ChangeTheme(Settings.Theme, Settings.ThemeModifier);

				if (reconnectRepo)
				{
					_repository = new NoteRepository(AppSettings.PATH_LOCALDB, this, Settings, Settings.ActiveAccount, dispatcher);
					_repository.Init();

					OnExplicitPropertyChanged("Repository");

					SelectedNote = Repository.Notes.FirstOrDefault();
				}
				else
				{
					_repository.ReplaceSettings(Settings);
				}

				Owner.TrayIcon.Visibility = (Settings.CloseToTray || Settings.MinimizeToTray) ? Visibility.Visible : Visibility.Collapsed;

				if (Settings.LaunchOnBoot)
				{
					var registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
					registryKey?.SetValue(string.Format(AppSettings.APPNAME_REG, Settings.ClientID), AppSettings.PATH_EXECUTABLE);
				}
				else
				{
					var registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
					if (registryKey?.GetValue(string.Format(AppSettings.APPNAME_REG, Settings.ClientID)) != null) registryKey.DeleteValue(string.Format(AppSettings.APPNAME_REG, Settings.ClientID));
				}

				// refresh Template
				if (refreshNotesViewTemplate) Owner.UpdateNotesViewComponent(Settings);

				if (refreshNotesCtrlView)  Owner.NotesViewControl.RefreshView();

				Owner.SetupScintilla(Settings);
				Owner.UpdateShortcuts(Settings);

				SearchText = string.Empty;

				App.Logger.Trace("Main", $"ChangeSettings took {sw.ElapsedMilliseconds}ms");
			}
			catch (Exception e)
			{
				App.Logger.Error("Main", "Apply Settings failed", e);
				ExceptionDialog.Show(Owner, "Apply Settings failed.\r\nSettings and Local notes could be in an invalid state.\r\nContinue with caution.", e, string.Empty);
			}
		}

		public void ChangeAccount(Guid newAccountUUID)
		{
			if (newAccountUUID == Repository.ConnectionID) return;

			var newSettings = Settings.Clone();
			newSettings.ActiveAccount = newSettings.Accounts.FirstOrDefault(a => a.ID == newAccountUUID);

			ChangeSettings(newSettings);
		}

		private void SelectedNoteChanged()
		{
			if (_lastSelectedNote != null) {_lastSelectedNote.PropertyChanged -= SelectedNotePropertyChanged; _lastSelectedNote=null; }
			
			if (SelectedNote != null && Settings != null && Settings.AutoSortTags)
			{
				SelectedNote.Tags.SynchronizeCollectionSafe(SelectedNote.Tags.OrderBy(p => p));
			}

			Owner.ResetScintillaScrollAndUndo();
			if (Settings != null) Owner.UpdateMargins(Settings);
			if (!PreventScintillaFocusLock.Value && Settings?.AutofocusScintilla == true) Owner.FocusScintillaDelayed();

			if (SelectedNote != null) ScintillaSearcher.Highlight(Owner.NoteEdit, SelectedNote, SearchText);

			if (Settings != null && (Settings.RememberScroll || Settings.RememberScrollPerSession)) Owner.ScrollScintilla(_scrollCache.Get(SelectedNote));

			if (Settings != null && Settings.RememberLastSelectedNote)
			{
				Settings.LastSelectedNote = SelectedNote?.UniqueName;
				Settings.LastSelectedFolder = SelectedFolderPath ?? DirectoryPath.Root();
			}
			RequestSettingsSave();

			if (SelectedNote != null) { SelectedNote.PropertyChanged += SelectedNotePropertyChanged; _lastSelectedNote = SelectedNote; }
		}

		private void SelectedNotePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(INote.IsLocked))
			{
				Owner.SetupScintilla(Settings);
			}
		}

		private void SelectedFolderPathChanged()
		{
			if (Settings != null && Settings.RememberLastSelectedNote)
			{
				Settings.LastSelectedNote = SelectedNote?.UniqueName;
				Settings.LastSelectedFolder = SelectedFolderPath ?? DirectoryPath.Root();
			}
			if (!string.IsNullOrEmpty(SearchText) && (Settings != null && Settings.ClearSearchOnFolderClick) && !(SelectedFolderPath == null || HierachicalBaseWrapper.IsSpecial(SelectedFolderPath)))
			{
				SearchText = ""; // clear serach on subfolder click 
			}
			RequestSettingsSave();
		}

		private void SettingsChanged()
		{
			if (Settings == null) return;
			
			ShortcutManager.UpdateSnippetCommands(Settings.Snippets.Data);
		}

		public void OnNoteChanged(NoteChangedEventArgs e) // only local changes
		{
			if (Settings.AutoSortTags && e.PropertyName == "Tags")
			{
				Application.Current.Dispatcher.BeginInvoke(new Action(() =>
				{
					SelectedNote.Tags.SynchronizeCollectionSafe(SelectedNote.Tags.OrderBy(p => p).ToList());
				}));
			}

			if (Settings.NoteSorting == SortingMode.ByModificationDate && !Owner.NotesViewControl.IsTopSortedNote(e.Note))
			{
				Owner.NotesViewControl.RefreshView();
			}
			else if (Settings.NoteSorting == SortingMode.ByName && e.PropertyName == "Title")
			{
				Owner.NotesViewControl.RefreshView();
			}
			else if (Settings.UseHierachicalNoteStructure && e.PropertyName == "Path")
			{
				Owner.NotesViewControl.RefreshView();
			}
			else if (Settings.SortByPinned && e.PropertyName == "IsPinned")
			{
				Owner.NotesViewControl.RefreshView();
			}
		}

		public void GridSplitterChanged()
		{
			RequestSettingsSave();
		}

		public void StartSync()
		{
			LastSynchronizedText = "[SYNCING]";
			SynchronizationState = SynchronizationState.Syncing;
		}

		public void SyncSuccess(DateTimeOffset now)
		{
			_lastSynchronized = now;
			LastSynchronizedText = now.ToLocalTime().ToString("HH:mm:ss");
			SynchronizationState = SynchronizationState.UpToDate;
		}

		public void OnSyncRequest()
		{
			SynchronizationState = SynchronizationState.NotSynced;
		}

		public void SyncError(List<Tuple<string, Exception>> errors)
		{
			if (errors.Count==0) return;

			if (_lastSynchronized != null)
			{
				LastSynchronizedText = _lastSynchronized.Value.ToLocalTime().ToString("HH:mm:ss");
			}
			else
			{
				LastSynchronizedText = "[ERROR]";
			}

			SynchronizationState = SynchronizationState.Error;

			App.Logger.Error("Sync", string.Join(Environment.NewLine, errors.Select(p => p.Item1)), string.Join("\r\n\r\n\r\n", errors.Select(p => p.Item2.ToString())));

			if (Settings.SuppressAllSyncProblemsPopup)
			{
				App.Logger.Info("Sync", "Suppress error display due to config [[SuppressAllSyncProblemsPopup]]");
			}
			else if (Settings.SuppressConnectionProblemPopup && errors.All(e => (e.Item2 as RestException)?.IsConnectionProblem == true))
			{
				App.Logger.Info("Sync", "Suppress error display due to config [[SuppressConnectionProblemPopup]]");
			}
			else
			{
				if (Owner.Visibility == Visibility.Hidden)
				{
					Owner.TrayIcon.ShowBalloonTip(
						"Synchronization failed", 
						string.Join(Environment.NewLine, errors.Select(p => p.Item1)), 
						BalloonIcon.Error);
				}
				else
				{
					SyncErrorDialog.Show(Owner, errors.Select(p => p.Item1).ToList(), errors.Select(p => p.Item2).ToList());
				}
			}
		}

		private void OnClosing(CancelEventArgs e)
		{
			if (Settings.CloseToTray && !_forceClose)
			{
				Owner.Hide();
				e.Cancel = true;
			}
		}

		private void OnClose(EventArgs args)
		{
			try
			{
				_repository.Shutdown();
			}
			catch (Exception e)
			{
				App.Logger.Error("Main", "Shutting down connection failed", e);
				ExceptionDialog.Show(Owner, "Shutting down connection failed.\r\nConnection will be forcefully aborted.", e, string.Empty);
				_repository.KillThread();
			}

			if (Settings.RememberScroll) _scrollCache.ForceSaveNow();

			if (Settings.RememberPositionAndSize || Settings.RememberWindowState)
			{
				SettingsHelper.ApplyWindowState(Owner, Settings, Settings.RememberPositionAndSize, Settings.RememberPositionAndSize, Settings.RememberWindowState);
				RequestSettingsSave();
			}

			if (_invSaveSettings.HasPendingRequests())
			{
				_invSaveSettings.CancelPendingRequests();
				SaveSettings();
			}

			Owner.MainWindow_OnClosed(args);
		}

		private void OnStateChanged(EventArgs e)
		{
			if (WindowState == WindowState.Minimized && Settings.MinimizeToTray)
			{
				Owner.Hide();
			}

			if (WindowState == WindowState.Minimized && Settings.LockOnMinimize && !Settings.IsReadOnlyMode)
			{
				Settings.IsReadOnlyMode = true;
				RequestSettingsSave();
			}
		}

		private void FilterNoteList()
		{
			var sn = SelectedNote;

			using (PreventScintillaFocusLock.Set())
			{
				Owner.NotesViewControl.RefreshView();
				if (Owner.NotesViewControl.Contains(sn))
					SelectedNote = sn;
				else
					SelectedNote = Owner.NotesViewControl.GetTopNote();
			}

			if (SelectedNote != null) ScintillaSearcher.Highlight(Owner.NoteEdit, SelectedNote, SearchText);
		}

		public void SetSelectedNoteWithoutFocus(INote n)
		{
			using (PreventScintillaFocusLock.Set())
			{
				SelectedNote = n;
			}
		}

		private void SaveSettings()
		{
			Settings?.Save();
		}

		public void RequestSettingsSave()
		{
			_invSaveSettings.Request();
		}

		public void OnScroll(int yoffset, int cursorPos)
		{
			if (Settings.RememberScroll) 
				_scrollCache.Set(SelectedNote, yoffset, cursorPos);
			else if (Settings.RememberScrollPerSession) 
				_scrollCache.SetNoSave(SelectedNote, yoffset, cursorPos);
		}

		public void ForceUpdateUIScroll()
		{
			if (Settings.RememberScroll) Owner.ScrollScintilla(_scrollCache.Get(SelectedNote));
		}

		private void CheckForUpdatesAsync()
		{
			#if DEBUG
			return;
			#endif

			try
			{
				Thread.Sleep(1000);
				var ghc = new GithubConnection();
				var r = ghc.GetLatestRelease(Settings.UpdateToPrerelease);

				if (r.Item1 > App.APP_VERSION)
				{
					Application.Current.Dispatcher.BeginInvoke(new Action(() => { UpdateWindow.Show(Owner, this, r.Item1, r.Item2, r.Item3); }));
				}
			}
			catch (Exception e)
			{
				App.Logger.Error("Main", "Updatecheck failed: Can't get latest version from github", e);
			}
		}

		private void UploadUsageStatsAsync()
		{
			try
			{
				Thread.Sleep(3000);
				var asc = new StatsConnection(Settings, Repository);
				asc.UploadStatistics(App.APP_VERSION);
			}
			catch (Exception e)
			{
				App.Logger.Error("Main", "Fatal error in UploadUsageStatsAsync", e);
			}
		}

		public void OnNewNoteDrop(IDataObject data)
		{
			try
			{
				var notepath = Owner.NotesViewControl.GetNewNotesPath();

				if (data.GetDataPresent(DataFormats.FileDrop, true))
				{
					string[] paths = data.GetData(DataFormats.FileDrop, true) as string[];
					foreach (var path in paths ?? new string[0])
					{
						var filename = Path.GetFileNameWithoutExtension(path) ?? "New note from unknown file";
						var filecontent = File.ReadAllText(path);

						SelectedNote = Repository.CreateNewNote(notepath);
						SelectedNote.Title = filename;
						SelectedNote.Text  = filecontent;
					}
				}
				else if (data.GetDataPresent(DataFormats.Text, true))
				{
					var notetitle   = "New note from drag&drop";
					var notecontent = data.GetData(DataFormats.Text, true) as string;
					if (!string.IsNullOrWhiteSpace(notecontent))
					{
						SelectedNote = Repository.CreateNewNote(notepath);
						SelectedNote.Title = notetitle;
						SelectedNote.Text  = notecontent;
					}
				}
			}
			catch (Exception ex)
			{
				ExceptionDialog.Show(Owner, "Drag&Drop failed", "Drag and Drop operation failed due to an internal error", ex);
			}
		}

		public void OnThemeChanged()
		{
			Owner.SetupScintilla(Settings);
		}

		public void ShowConflictResolutionDialog(string uuid, string txt0, string ttl0, List<string> tgs0, DirectoryPath ndp0, string txt1, string ttl1, List<string> tgs1, DirectoryPath ndp1)
		{
			ConflictWindow.Show(Repository, Owner, uuid, Tuple.Create(txt0, ttl0, tgs0, ndp0), Tuple.Create(txt1, ttl1, tgs1, ndp1));
		}
	}
}
