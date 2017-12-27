using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using AlephNote.Common.AlephXMLSerialization;
using AlephNote.Common.MVVM;
using AlephNote.Common.Plugins;
using AlephNote.Common.Settings.Types;
using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Objects.AXML;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Common.Settings
{
	// ReSharper disable RedundantThisQualifier
	// ReSharper disable CompareOfFloatsByEqualityOperator
	public class AppSettings : ObservableObject, IAlephSerializable
	{
		public const string ENCRYPTION_KEY = @"jcgkZJvoykjpoGkDWHqiNoXoLZRJxpdb";

		public const string TAG_MARKDOWN = "markdown";
		public const string TAG_LIST     = "list";

		[AlephXMLField]
		public ConfigInterval SynchronizationFrequency { get { return _synchronizationFreq; } set { _synchronizationFreq = value; OnPropertyChanged(); } }
		private ConfigInterval _synchronizationFreq = ConfigInterval.Sync15Min;

		[AlephXMLField]
		public bool ProxyEnabled { get { return _proxyEnabled; } set { _proxyEnabled = value; OnPropertyChanged(); } }
		private bool _proxyEnabled = false;

		[AlephXMLField]
		public string ProxyHost { get { return _proxyHost; } set { _proxyHost = value; OnPropertyChanged(); } }
		private string _proxyHost = string.Empty;

		[AlephXMLField]
		public int? ProxyPort { get { return _proxyPort; } set { _proxyPort = value; OnPropertyChanged(); } }
		private int? _proxyPort = null;

		[AlephXMLField]
		public string ProxyUsername { get { return _proxyUsername; } set { _proxyUsername = value; OnPropertyChanged(); } }
		private string _proxyUsername = string.Empty;

		[AlephXMLField(Encrypted=true)]
		public string ProxyPassword { get { return _proxyPassword; } set { _proxyPassword = value; OnPropertyChanged(); } }
		private string _proxyPassword = string.Empty;

		[AlephXMLField]
		public bool MinimizeToTray { get { return _minimizeToTray; } set { _minimizeToTray = value; OnPropertyChanged(); } }
		private bool _minimizeToTray = true;

		[AlephXMLField]
		public bool CloseToTray { get { return _closeToTray; } set { _closeToTray = value; OnPropertyChanged(); } }
		private bool _closeToTray = false;

		[AlephXMLField]
		public string TitleFontFamily { get { return _titleFontFamily; } set { _titleFontFamily = value; OnPropertyChanged(); } }
		private string _titleFontFamily = string.Empty;

		[AlephXMLField]
		public FontModifier TitleFontModifier { get { return _titleFontModifier; } set { _titleFontModifier = value; OnPropertyChanged(); } }
		private FontModifier _titleFontModifier = FontModifier.Bold;

		[AlephXMLField]
		public FontSize TitleFontSize { get { return _titleFontSize; } set { _titleFontSize = value; OnPropertyChanged(); } }
		private FontSize _titleFontSize = FontSize.Size16;

		[AlephXMLField]
		public string NoteFontFamily { get { return _noteFontFamily; } set { _noteFontFamily = value; OnPropertyChanged(); } }
		private string _noteFontFamily = string.Empty;

		[AlephXMLField]
		public FontModifier NoteFontModifier { get { return _noteFontModifier; } set { _noteFontModifier = value; OnPropertyChanged(); } }
		private FontModifier _noteFontModifier = FontModifier.Normal;

		[AlephXMLField]
		public FontSize NoteFontSize { get { return _noteFontSize; } set { _noteFontSize = value; OnPropertyChanged(); } }
		private FontSize _noteFontSize = FontSize.Size08;

		[AlephXMLField]
		public string ListFontFamily { get { return _listFontFamily; } set { _listFontFamily = value; OnPropertyChanged(); } }
		private string _listFontFamily = string.Empty;

		[AlephXMLField]
		public FontModifier ListFontModifier { get { return _listFontModifier; } set { _listFontModifier = value; OnPropertyChanged(); } }
		private FontModifier _listFontModifier = FontModifier.Normal;

		[AlephXMLField]
		public FontSize ListFontSize { get { return _listFontSize; } set { _listFontSize = value; OnPropertyChanged(); } }
		private FontSize _listFontSize = FontSize.Size12;

		[AlephXMLField]
		public bool SciLineNumbers { get { return _sciLineNumbers; } set { _sciLineNumbers = value; OnPropertyChanged(); } }
		private bool _sciLineNumbers = false;

		[AlephXMLField]
		public bool SciRectSelection { get { return _sciRectSelection; } set { _sciRectSelection = value; OnPropertyChanged(); } }
		private bool _sciRectSelection = false;

		[AlephXMLField]
		public bool SciZoomable { get { return _sciZoomable; } set { _sciZoomable = value; OnPropertyChanged(); } }
		private bool _sciZoomable = true;

		[AlephXMLField]
		public bool SciUseTabs { get { return _sciUseTabs; } set { _sciUseTabs = value; OnPropertyChanged(); } }
		private bool _sciUseTabs = true;
		
		[AlephXMLField]
		public bool SciWordWrap { get { return _sciWordWrap; } set { _sciWordWrap = value; OnPropertyChanged(); } }
		private bool _sciWordWrap = false;

		[AlephXMLField]
		public bool SciShowWhitespace { get { return _sciShowWhitespace; } set { _sciShowWhitespace = value; OnPropertyChanged(); } }
		private bool _sciShowWhitespace = false;

		[AlephXMLField]
		public bool SciShowEOL { get { return _sciShowEOL; } set { _sciShowEOL = value; OnPropertyChanged(); } }
		private bool _sciShowEOL = false;

		[AlephXMLField]
		public int SciTabWidth { get { return _sciTabWidth; } set { _sciTabWidth = value; OnPropertyChanged(); } }
		private int _sciTabWidth = 4;

		[AlephXMLField]
		public bool SciScrollAfterLastLine { get { return _sciScrollAfterLastLine; } set { _sciScrollAfterLastLine = value; OnPropertyChanged(); } }
		private bool _sciScrollAfterLastLine = false;

		[AlephXMLField]
		public int StartupPositionX { get { return _startupPositionX; } set { _startupPositionX = value; OnPropertyChanged(); } }
		private int _startupPositionX = 64;

		[AlephXMLField]
		public int StartupPositionY { get { return _startupPositionY; } set { _startupPositionY = value; OnPropertyChanged(); } }
		private int _startupPositionY = 64;

		[AlephXMLField]
		public int StartupPositionWidth { get { return _startupPositionWidth; } set { _startupPositionWidth = value; OnPropertyChanged(); } }
		private int _startupPositionWidth = 580;

		[AlephXMLField]
		public int StartupPositionHeight { get { return _startupPositionHeight; } set { _startupPositionHeight = value; OnPropertyChanged(); } }
		private int _startupPositionHeight = 565;

		[AlephXMLField]
		public ExtendedWindowStartupLocation StartupLocation { get { return _startupLocation; } set { _startupLocation = value; OnPropertyChanged(); } }
		private ExtendedWindowStartupLocation _startupLocation = ExtendedWindowStartupLocation.ScreenBottomLeft;
		
		[AlephXMLField]
		public ExtendedWindowState StartupState { get { return _startupState; } set { _startupState = value; OnPropertyChanged(); } }
		private ExtendedWindowState _startupState = ExtendedWindowState.Normal;

		[AlephXMLField]
		public bool LaunchOnBoot { get { return _launchOnBoot; } set { _launchOnBoot = value; OnPropertyChanged(); } }
		private bool _launchOnBoot = false;

		[AlephXMLField]
		public SortingMode NoteSorting { get { return _noteSorting; } set { _noteSorting = value; OnPropertyChanged(); } }
		private SortingMode _noteSorting = SortingMode.ByModificationDate;

		[AlephXMLField]
		public int SciZoom { get { return _sciZoom; } set { _sciZoom = value; OnPropertyChanged(); } }
		private int _sciZoom = 1;

		[AlephXMLField]
		public double OverviewListWidth { get { return _overviewListWidth; } set { _overviewListWidth = value; OnPropertyChanged(); } }
		private double _overviewListWidth = 150;

		[AlephXMLField]
		public NotePreviewStyle NotePreviewStyle { get { return _notePreviewStyle; } set { _notePreviewStyle = value; OnPropertyChanged(); } }
		private NotePreviewStyle _notePreviewStyle = NotePreviewStyle.Extended;

		[AlephXMLField]
		public ConflictResolutionStrategyConfig ConflictResolution { get { return _conflictResolution; } set { _conflictResolution = value; OnPropertyChanged(); } }
		private ConflictResolutionStrategyConfig _conflictResolution = ConflictResolutionStrategyConfig.UseClientCreateConflictFile;

		[AlephXMLField]
		public bool DocSearchEnabled { get { return _docSearchEnabled; } set { _docSearchEnabled = value; OnPropertyChanged(); } }
		private bool _docSearchEnabled = true;

		[AlephXMLField]
		public bool DocSearchCaseSensitive { get { return _docSearchCaseSensitive; } set { _docSearchCaseSensitive = value; OnPropertyChanged(); } }
		private bool _docSearchCaseSensitive = false;

		[AlephXMLField]
		public bool DocSearchWholeWord { get { return _docSearchWholeWord; } set { _docSearchWholeWord = value; OnPropertyChanged(); } }
		private bool _docSearchWholeWord = false;

		[AlephXMLField]
		public bool DocSearchRegex { get { return _docSearchRegex; } set { _docSearchRegex = value; OnPropertyChanged(); } }
		private bool _docSearchRegex = false;

		[AlephXMLField]
		public bool DocSearchLiveSearch { get { return _docSearchLiveSearch; } set { _docSearchLiveSearch = value; OnPropertyChanged(); } }
		private bool _docSearchLiveSearch = true;

		[AlephXMLField]
		public SciRegexEngine DocSearchRegexEngine { get { return _docSearchRegexEngine; } set { _docSearchRegexEngine = value; OnPropertyChanged(); } }
		private SciRegexEngine _docSearchRegexEngine = SciRegexEngine.CPlusPlus;

		[AlephXMLField]
		public bool CheckForUpdates { get { return _checkForUpdates; } set { _checkForUpdates = value; OnPropertyChanged(); } }
		private bool _checkForUpdates = true;

		[AlephXMLField]
		public bool DoGitMirror { get { return _doGitMirror; } set { _doGitMirror = value; OnPropertyChanged(); } }
		private bool _doGitMirror = false;

		[AlephXMLField]
		public string GitMirrorPath { get { return _gitMirrorPath; } set { _gitMirrorPath = value; OnPropertyChanged(); } }
		private string _gitMirrorPath = string.Empty;

		[AlephXMLField]
		public string GitMirrorFirstName { get { return _gitMirrorFirstName; } set { _gitMirrorFirstName = value; OnPropertyChanged(); } }
		private string _gitMirrorFirstName = "AlephNote";

		[AlephXMLField]
		public string GitMirrorLastName { get { return _gitMirrorLastName; } set { _gitMirrorLastName = value; OnPropertyChanged(); } }
		private string _gitMirrorLastName = "Git";

		[AlephXMLField]
		public string GitMirrorMailAddress { get { return _gitMirrorMailAddress; } set { _gitMirrorMailAddress = value; OnPropertyChanged(); } }
		private string _gitMirrorMailAddress = "auto@example.com";

		[AlephXMLField]
		public bool GitMirrorDoPush { get { return _gitMirrorDoPush; } set { _gitMirrorDoPush = value; OnPropertyChanged(); } }
		private bool _gitMirrorDoPush = false;

		[AlephXMLField]
		public bool GitMirrorSubfolders { get { return _gitMirrorSubfolders; } set { _gitMirrorSubfolders = value; OnPropertyChanged(); } }
		private bool _gitMirrorSubfolders = false;

		[AlephXMLField]
		public RemoteStorageAccount ActiveAccount { get { return _activeAccount; } set { _activeAccount = value; OnPropertyChanged(); } }
		private RemoteStorageAccount _activeAccount = null;

		[AlephXMLField]
		public ObservableCollectionNoReset<RemoteStorageAccount> Accounts { get { return _accounts; } set { _accounts = value; OnPropertyChanged(); } }
		private ObservableCollectionNoReset<RemoteStorageAccount> _accounts = new ObservableCollectionNoReset<RemoteStorageAccount>();

		[AlephXMLField]
		public LinkHighlightMode LinkMode { get { return _linkMode; } set { _linkMode = value; OnPropertyChanged(); } }
		private LinkHighlightMode _linkMode = LinkHighlightMode.ControlClick;

		[AlephXMLField]
		public MarkdownHighlightMode MarkdownMode { get { return _markdownMode; } set { _markdownMode = value; OnPropertyChanged(); } }
		private MarkdownHighlightMode _markdownMode = MarkdownHighlightMode.WithTag;

		[AlephXMLField]
		public ListHighlightMode ListMode { get { return _listMode; } set { _listMode = value; OnPropertyChanged(); } }
		private ListHighlightMode _listMode = ListHighlightMode.WithTag;

		[AlephXMLField]
		public bool TagAutocomplete { get { return _tagAutocomplete; } set { _tagAutocomplete = value; OnPropertyChanged(); } }
		private bool _tagAutocomplete = true;

		[AlephXMLField]
		public bool AlwaysOnTop { get { return _alwaysOnTop; } set { _alwaysOnTop = value; OnPropertyChanged(); } }
		private bool _alwaysOnTop = false;

		[AlephXMLField]
		public KeyValueCustomList<SnippetDefinition> Snippets { get { return _snippets; } set { _snippets = value; OnPropertyChanged(); } }
		private KeyValueCustomList<SnippetDefinition> _snippets = CreateDefaultSnippetList();

		[AlephXMLField]
		public string LastSelectedNote { get { return _lastSelectedNote; } set { _lastSelectedNote = value; OnPropertyChanged(); } }
		private string _lastSelectedNote = null;

		[AlephXMLField]
		public bool RememberLastSelectedNote { get { return _rememberLastSelectedNote; } set { _rememberLastSelectedNote = value; OnPropertyChanged(); } }
		private bool _rememberLastSelectedNote = true;

		[AlephXMLField]
		public bool AutoSortTags { get { return _autoSortTags; } set { _autoSortTags = value; OnPropertyChanged(); } }
		private bool _autoSortTags = true;

		[AlephXMLField]
		public KeyValueFlatCustomList<ShortcutDefinition> Shortcuts { get { return _shortcuts; } set { _shortcuts = value; OnPropertyChanged(); } }
		private KeyValueFlatCustomList<ShortcutDefinition> _shortcuts = CreateDefaultShortcutList();

		[AlephXMLField]
		public Guid ClientID { get { return _clientID; } set { _clientID = value; OnPropertyChanged(); } }
		private Guid _clientID = Guid.NewGuid();

		[AlephXMLField]
		public bool SendAnonStatistics { get { return _sendAnonStatistics; } set { _sendAnonStatistics = value; OnPropertyChanged(); } }
		private bool _sendAnonStatistics = true;

		[AlephXMLField]
		public bool RememberScroll { get { return _rememberScroll; } set { _rememberScroll = value; OnPropertyChanged(); } }
		private bool _rememberScroll = false;

		[AlephXMLField]
		public bool UseHierachicalNoteStructure { get { return _useHierachicalNoteStructure; } set { _useHierachicalNoteStructure = value; OnPropertyChanged(); } }
		private bool _useHierachicalNoteStructure = false;

		[AlephXMLField]
		public bool EmulateHierachicalStructure { get { return _emulateHierachicalStructure; } set { _emulateHierachicalStructure = value; OnPropertyChanged(); } }
		private bool _emulateHierachicalStructure = true;

		[AlephXMLField]
		public HierachicalStructureSeperator HStructureSeperator { get { return _hStructureSeperator; } set { _hStructureSeperator = value; OnPropertyChanged(); } }
		private HierachicalStructureSeperator _hStructureSeperator = HierachicalStructureSeperator.SeperatorForwardSlash;

		[AlephXMLField]
		public double NotesViewFolderHeight { get { return _notesViewFolderHeight; } set { _notesViewFolderHeight = value; OnPropertyChanged(); } }
		private double _notesViewFolderHeight = 120;

		[AlephXMLField]
		public DirectoryPath LastSelectedFolder { get { return _lastSelectedFolder; } set { _lastSelectedFolder = value; OnPropertyChanged(); } }
		private DirectoryPath _lastSelectedFolder = DirectoryPath.Root();

		[AlephXMLField]
		public bool SmoothScrollNotesView { get { return _smoothScrollNotesView; } set { _smoothScrollNotesView = value; OnPropertyChanged(); } }
		private bool _smoothScrollNotesView = false;

		[AlephXMLField]
		public bool AutofocusScintilla { get { return _autofocusScintilla; } set { _autofocusScintilla = value; OnPropertyChanged(); } }
		private bool _autofocusScintilla = true;

		private static readonly AlephXMLSerializer<AppSettings> _serializer = new AlephXMLSerializer<AppSettings>("configuration");

		private readonly string _path;

		private AppSettings(string path)
		{
			_path = path;
		}

		public static AppSettings CreateEmpty(string path)
		{
			var r = new AppSettings(path);

			var defplugin = PluginManagerSingleton.Inst.GetDefaultPlugin();
			r._activeAccount = new RemoteStorageAccount(Guid.NewGuid(), defplugin, defplugin.CreateEmptyRemoteStorageConfiguration());

			r._accounts.Add(r._activeAccount);

			return r;
		}

		public void Save()
		{
			File.WriteAllText(_path, Serialize());
		}

		public static AppSettings Load(string path)
		{
			return Deserialize(File.ReadAllText(path), path);
		}

		public string Serialize()
		{
			return _serializer.Serialize(this, AlephXMLSerializer<AppSettings>.DEFAULT_SERIALIZATION_SETTINGS);
		}

		public static AppSettings Deserialize(string xml, string path)
		{
			var r = CreateEmpty(path);
			_serializer.Deserialize(r, xml, AlephXMLSerializer<AppSettings>.DEFAULT_SERIALIZATION_SETTINGS);
			return r;
		}

		public void OnBeforeSerialize()
		{
			//
		}

		public void OnAfterDeserialize()
		{
			_activeAccount = _accounts.FirstOrDefault(a => a.ID == _activeAccount.ID);
			if (_activeAccount == null) throw new Exception("Deserialization error: ActiveAccount not found in AccountList");
		}

		public AppSettings Clone()
		{
			var r = CreateEmpty(_path);

			_serializer.Clone(this, r);

			return r;
		}

		public bool IsEqual(AppSettings other)
		{
			return _serializer.IsEqual(this, other);
		}

		public void RemoveAccount(RemoteStorageAccount acc)
		{
			if (_activeAccount == acc) ActiveAccount = Accounts.Except(new[]{ acc }).FirstOrDefault();

			Accounts.Remove(acc);

			OnPropertyChanged("Accounts");
		}

		public void AddAccountAndSetActive(RemoteStorageAccount acc)
		{
			Accounts.Add(acc);
			ActiveAccount = acc;

			OnPropertyChanged("Accounts");
		}

		public IWebProxy CreateProxy()
		{
			if (ProxyEnabled)
			{
				if (string.IsNullOrWhiteSpace(ProxyUsername) && string.IsNullOrWhiteSpace(ProxyPassword))
				{
					return PluginManagerSingleton.Inst.GetProxyFactory().Build(ProxyHost, ProxyPort ?? 443);
				}
				else
				{
					return PluginManagerSingleton.Inst.GetProxyFactory().Build(ProxyHost, ProxyPort ?? 443, ProxyUsername, ProxyPassword);
				}
			}
			else
			{
				return PluginManagerSingleton.Inst.GetProxyFactory().Build();
			}
		}

		public int GetSyncDelay()
		{
			switch (SynchronizationFrequency)
			{
				case ConfigInterval.Sync01Min:  return       1 * 60 * 1000;
				case ConfigInterval.Sync02Min:  return       2 * 60 * 1000;
				case ConfigInterval.Sync05Min:  return       5 * 60 * 1000;
				case ConfigInterval.Sync10Min:  return      10 * 60 * 1000;
				case ConfigInterval.Sync15Min:  return      15 * 60 * 1000;
				case ConfigInterval.Sync30Min:  return      30 * 60 * 1000;
				case ConfigInterval.Sync01Hour: return  1 * 60 * 60 * 1000;
				case ConfigInterval.Sync02Hour: return  1 * 60 * 60 * 1000;
				case ConfigInterval.Sync06Hour: return  6 * 60 * 60 * 1000;
				case ConfigInterval.Sync12Hour: return 12 * 60 * 60 * 1000;
				case ConfigInterval.SyncManual: return int.MaxValue - 1000;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static readonly IComparer<INote> COMPARER_NONE  = ProjectionComparer.Create<INote, string>(n => n.GetUniqueName());
		public static readonly IComparer<INote> COMPARER_NAME  = ProjectionComparer.Create<INote, string>(n => n.Title);
		public static readonly IComparer<INote> COMPARER_CDATE = ProjectionComparer.Create<INote, DateTimeOffset>(n => n.CreationDate, true);
		public static readonly IComparer<INote> COMPARER_MDATE = ProjectionComparer.Create<INote, DateTimeOffset>(n => n.ModificationDate, true);

		public IComparer<INote> GetNoteComparator()
		{
			switch (NoteSorting)
			{
				case SortingMode.None:               return COMPARER_NONE;
				case SortingMode.ByName:             return COMPARER_NAME;
				case SortingMode.ByCreationDate:     return COMPARER_CDATE;
				case SortingMode.ByModificationDate: return COMPARER_MDATE;

				default: throw new ArgumentOutOfRangeException();
			}
		}

		public HierachyEmulationConfig GetHierachicalConfig()
		{
			bool enabled = UseHierachicalNoteStructure && EmulateHierachicalStructure;

			var sep = StructureSeperatorHelper.GetSeperator(HStructureSeperator);
			var esc = StructureSeperatorHelper.GetEscapeChar(HStructureSeperator);

			return new HierachyEmulationConfig(enabled, sep, esc);
		}

		private static KeyValueCustomList<SnippetDefinition> CreateDefaultSnippetList()
		{
			return new KeyValueCustomList<SnippetDefinition>(new[]
			{
				Tuple.Create("date", new SnippetDefinition("Current Date", "{now:yyyy-MM-dd}")),
				Tuple.Create("time", new SnippetDefinition("Current Time", "{now:HH:mm:ss}")),
				Tuple.Create("date+time", new SnippetDefinition("Current Date & Time", "{now:yyyy-MM-dd HH:mm:ss}")),
			},
			SnippetDefinition.DEFAULT);
		}

		private static KeyValueFlatCustomList<ShortcutDefinition> CreateDefaultShortcutList()
		{
			return new KeyValueFlatCustomList<ShortcutDefinition>(new[]
			{
				Tuple.Create("NewNote",             new ShortcutDefinition(AlephShortcutScope.Window,     AlephModifierKeys.Control, AlephKey.N)),       // v1.6.0
				Tuple.Create("SaveAndSync",         new ShortcutDefinition(AlephShortcutScope.Window,     AlephModifierKeys.Control, AlephKey.S)),       // v1.6.0
				Tuple.Create("DocumentSearch",      new ShortcutDefinition(AlephShortcutScope.NoteEdit,   AlephModifierKeys.Control, AlephKey.F)),       // v1.6.0
				Tuple.Create("CloseDocumentSearch", new ShortcutDefinition(AlephShortcutScope.Window,     AlephModifierKeys.None,    AlephKey.Escape)),  // v1.6.0
				Tuple.Create("DeleteNote",          new ShortcutDefinition(AlephShortcutScope.NoteList,   AlephModifierKeys.None,    AlephKey.Delete)),  // v1.6.0
				Tuple.Create("AppExit",             new ShortcutDefinition(AlephShortcutScope.Window,     AlephModifierKeys.Alt,     AlephKey.F4)),      // v1.6.0
				Tuple.Create("DeleteFolder",        new ShortcutDefinition(AlephShortcutScope.FolderList, AlephModifierKeys.None,    AlephKey.Delete)),  // v1.6.4
				Tuple.Create("RenameFolder",        new ShortcutDefinition(AlephShortcutScope.FolderList, AlephModifierKeys.None,    AlephKey.F2)),      // v1.6.4
			}, 
			ShortcutDefinition.DEFAULT);
		}

		public void Migrate(Version from, Version to, IAlephLogger log)
		{
			var v1_6_4 = new Version(1, 6, 4, 0);

			log.Info("AppSettings", $"Migrate settings from {from} to {to}");

			if (from < v1_6_4)
			{
				log.Info("AppSettings", "(Migration) Insert shortcut for [DeleteFolder]");
				Shortcuts = Shortcuts.Concat(Tuple.Create("DeleteFolder", new ShortcutDefinition(AlephShortcutScope.FolderList, AlephModifierKeys.None, AlephKey.Delete)));
			}
			if (from < v1_6_4)
			{
				log.Info("AppSettings", "(Migration) Insert shortcut for [RenameFolder]");
				Shortcuts = Shortcuts.Concat(Tuple.Create("RenameFolder", new ShortcutDefinition(AlephShortcutScope.FolderList, AlephModifierKeys.None, AlephKey.F2)));
			}
		}
	}
}