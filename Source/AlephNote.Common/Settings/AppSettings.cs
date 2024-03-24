﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using AlephNote.Common.AlephXMLSerialization;
using AlephNote.Common.Settings.Types;
using AlephNote.Common.Shortcuts;
using AlephNote.Common.Util;
using AlephNote.PluginInterface;
using AlephNote.PluginInterface.AppContext;
using AlephNote.PluginInterface.Objects;
using AlephNote.PluginInterface.Objects.AXML;
using AlephNote.PluginInterface.Util;
using MSHC.WPF.MVVM;

namespace AlephNote.Common.Settings
{
	// ReSharper disable RedundantThisQualifier
	// ReSharper disable CompareOfFloatsByEqualityOperator
	public class AppSettings : ObservableObject, IAlephSerializable, IReadonlyAlephSettings
	{
		public const int DEFAULT_INITIALDOWNLOAD_PARALLELISM_LEVEL     = 10;
		public const int DEFAULT_INITIALDOWNLOAD_PARALLELISM_THRESHOLD = 100;

		public static readonly string PATH_SETTINGS       = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"noteapp.config");
		public static readonly string PATH_SCROLLCACHE    = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"noteapp.scrollcache.config");
		public static readonly string PATH_GCCACHE        = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"noteapp.gitcleancache.config");
		public static readonly string PATH_HIERARCHYCACHE = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"noteapp.hierarchycache.config");
		public static readonly string PATH_LOCALDB        = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".notes");
		public static readonly string APPNAME_REG         = "AlephNoteApp_{0:N}";
		public static readonly string PATH_EXECUTABLE     = GetExePath();

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
		private bool _sciRectSelection = true;

		[AlephXMLField] // Only allowed when (SciRectSelection == true)
		public bool SciMultiSelection { get { return _sciMultiSelection; } set { _sciMultiSelection = value; OnPropertyChanged(); } }
		private bool _sciMultiSelection = false;

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
		public bool RememberPositionAndSize { get { return _rememberPositionAndSize; } set { _rememberPositionAndSize = value; OnPropertyChanged(); } }
		private bool _rememberPositionAndSize = false;

		[AlephXMLField]
		public bool RememberWindowState { get { return _rememberWindowState; } set { _rememberWindowState = value; OnPropertyChanged(); } }
		private bool _rememberWindowState = false;

		[AlephXMLField]
		public ExtendedWindowStartupLocation StartupLocation { get { return _startupLocation; } set { _startupLocation = value; OnPropertyChanged(); } }
		private ExtendedWindowStartupLocation _startupLocation = ExtendedWindowStartupLocation.ScreenBottomLeft;
		
		[AlephXMLField]
		public ExtendedWindowState StartupState { get { return _startupState; } set { _startupState = value; OnPropertyChanged(); } }
		private ExtendedWindowState _startupState = ExtendedWindowState.Normal;

		[AlephXMLField]
		public bool LaunchOnBoot { get { return _launchOnBoot; } set { _launchOnBoot = value; OnPropertyChanged(); } }
		private bool _launchOnBoot = false;

		[AlephXMLField(RefreshNotesControlView=true)]
		public SortingMode NoteSorting { get { return _noteSorting; } set { _noteSorting = value; OnPropertyChanged(); } }
		private SortingMode _noteSorting = SortingMode.ByModificationDate;

		[AlephXMLField(RefreshNotesControlView=true)]
		public bool SortByPinned { get { return _sortByPinned; } set { _sortByPinned = value; OnPropertyChanged(); } }
		private bool _sortByPinned = true;

		[AlephXMLField]
		public int SciZoom { get { return _sciZoom; } set { _sciZoom = value; OnPropertyChanged(); } }
		private int _sciZoom = 1;

		[AlephXMLField]
		public double OverviewListWidth { get { return _overviewListWidth; } set { _overviewListWidth = value; OnPropertyChanged(); } }
		private double _overviewListWidth = 150;

		[AlephXMLField]
		public NotePreviewStyle NotePreviewStyle { get { return _notePreviewStyle; } set { _notePreviewStyle = value; OnPropertyChanged(); } }
		private NotePreviewStyle _notePreviewStyle = NotePreviewStyle.Extended;

		[AlephXMLField(ReconnectRepo=true)]
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
		public int GitMirrorAutoGC { get { return _gitMirrorAutoGC; } set { _gitMirrorAutoGC = value; OnPropertyChanged(); } }
		private int _gitMirrorAutoGC = 0;

		[AlephXMLField]
		public bool GitMirrorSubfolders { get { return _gitMirrorSubfolders; } set { _gitMirrorSubfolders = value; OnPropertyChanged(); } }
		private bool _gitMirrorSubfolders = false;

		[AlephXMLField(ReconnectRepo=true)]
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
		private bool _autoSortTags = false;

		[AlephXMLField]
		public KeyValueFlatCustomList<ShortcutDefinition> Shortcuts { get { return _shortcuts; } set { _shortcuts = value; OnPropertyChanged(); } }
		private KeyValueFlatCustomList<ShortcutDefinition> _shortcuts = ShortcutManager.CreateDefaultShortcutList();

		[AlephXMLField]
		public Guid ClientID { get { return _clientID; } set { _clientID = value; OnPropertyChanged(); } }
		private Guid _clientID = Guid.NewGuid();

		[AlephXMLField]
		public bool SendAnonStatistics { get { return _sendAnonStatistics; } set { _sendAnonStatistics = value; OnPropertyChanged(); } }
		private bool _sendAnonStatistics = true;

		[AlephXMLField]
		public bool RememberScroll { get { return _rememberScroll; } set { _rememberScroll = value; OnPropertyChanged(); } }
		private bool _rememberScroll = false;

		[AlephXMLField(ReconnectRepo=true, RefreshNotesViewTemplate=true, XMLName="UseHierachicalNoteStructure")]
		public bool UseHierarchicalNoteStructure { get { return _useHierarchicalNoteStructure; } set { _useHierarchicalNoteStructure = value; OnPropertyChanged(); } }
		private bool _useHierarchicalNoteStructure = false;

		[AlephXMLField(ReconnectRepo=true, XMLName="EmulateHierachicalStructure")]
		public bool EmulateHierarchicalStructure { get { return _emulateHierarchicalStructure; } set { _emulateHierarchicalStructure = value; OnPropertyChanged(); } }
		private bool _emulateHierarchicalStructure = true;

		[AlephXMLField(ReconnectRepo=true)]
		public HierarchicalStructureSeperator HStructureSeperator { get { return _hStructureSeperator; } set { _hStructureSeperator = value; OnPropertyChanged(); } }
		private HierarchicalStructureSeperator _hStructureSeperator = HierarchicalStructureSeperator.SeperatorForwardSlash;

		[AlephXMLField]
		public double NotesViewFolderHeight { get { return _notesViewFolderHeight; } set { _notesViewFolderHeight = value; OnPropertyChanged(); } }
		private double _notesViewFolderHeight = 120;

		[AlephXMLField]
		public DirectoryPath LastSelectedFolder { get { return _lastSelectedFolder; } set { _lastSelectedFolder = value; OnPropertyChanged(); } }
		private DirectoryPath _lastSelectedFolder = DirectoryPath.Root();

		[AlephXMLField]
		public bool SmoothScrollNotesView { get { return _smoothScrollNotesView; } set { _smoothScrollNotesView = value; OnPropertyChanged(); } }
		private bool _smoothScrollNotesView = false; // true = performance problems with many notes

		[AlephXMLField]
		public bool AutofocusScintilla { get { return _autofocusScintilla; } set { _autofocusScintilla = value; OnPropertyChanged(); } }
		private bool _autofocusScintilla = false; // TODO use FocusTarget for more customization (migrate in AutoUpdater)

		[AlephXMLField]
		public bool FixScintillaScrollMessages { get { return _fixScintillaScrollMessages; } set { _fixScintillaScrollMessages = value; OnPropertyChanged(); } }
		private bool _fixScintillaScrollMessages = true;

		[AlephXMLField]
		public bool DeepFolderView { get { return _deepFolderView; } set { _deepFolderView = value; OnPropertyChanged(); } }
		private bool _deepFolderView = true;

		[AlephXMLField]
		public bool FolderViewShowRootNode { get { return _folderViewShowRootNode; } set { _folderViewShowRootNode = value; OnPropertyChanged(); } }
		private bool _folderViewShowRootNode = false;

		[AlephXMLField]
		public bool FolderViewShowEmptyPathNode { get { return _folderViewShowEmptyPathNode; } set { _folderViewShowEmptyPathNode = value; OnPropertyChanged(); } }
		private bool _folderViewShowEmptyPathNode = false;

		[AlephXMLField]
		public bool FolderViewShowAllNotesNode { get { return _folderViewAllNotesNode; } set { _folderViewAllNotesNode = value; OnPropertyChanged(); } }
		private bool _folderViewAllNotesNode = true;

		[AlephXMLField(RefreshNotesTheme=true)]
		public string Theme { get { return _theme; } set { _theme = value; OnPropertyChanged(); } }
		private string _theme = "default.xml";
		
		[AlephXMLField(RefreshNotesTheme=true)]
		public HashSet<string> ThemeModifier { get { return _themeModifier; } set { _themeModifier = value; OnPropertyChanged(); } }
		private HashSet<string> _themeModifier = new HashSet<string>();

		[AlephXMLField]
		public bool IsReadOnlyMode { get { return _isReadonlyMode; } set { _isReadonlyMode = value; OnPropertyChanged(); } }
		private bool _isReadonlyMode = false;

		[AlephXMLField]
		public bool ShowReadonlyLock { get { return _showReadonlyLock; } set { _showReadonlyLock = value; OnPropertyChanged(); } }
		private bool _showReadonlyLock = false;
		
		[AlephXMLField]
		public bool LockOnStartup { get { return _lockOnStartup; } set { _lockOnStartup = value; OnPropertyChanged(); } }
		private bool _lockOnStartup = false;

		[AlephXMLField]
		public bool LockOnMinimize { get { return _lockOnMinimize; } set { _lockOnMinimize = value; OnPropertyChanged(); } }
		private bool _lockOnMinimize = false;

		[AlephXMLField(IsAdvanced = true)]
		public bool UpdateToPrerelease { get { return _updateToPrerelease; } set { _updateToPrerelease = value; OnPropertyChanged(); } }
		private bool _updateToPrerelease = false;
		
		[AlephXMLField]
		public bool ClearSearchOnFolderClick { get { return _clearSearchOnFolderClick; } set { _clearSearchOnFolderClick = value; OnPropertyChanged(); } }
		private bool _clearSearchOnFolderClick = true;
		
		[AlephXMLField(ReconnectRepo=true)]
		public bool UseRawFolderRepo { get { return _useRawFolderRepo; } set { _useRawFolderRepo = value; OnPropertyChanged(); } }
		private bool _useRawFolderRepo = false;
		
		[AlephXMLField(ReconnectRepo=true)]
		public string RawFolderRepoPath { get { return _rawFolderRepoPath; } set { _rawFolderRepoPath = value; OnPropertyChanged(); } }
		private string _rawFolderRepoPath = "";
		
		[AlephXMLField(ReconnectRepo=true)]
		public bool RawFolderRepoAllowDeletion { get { return _rawFolderRepoAllowDeletion; } set { _rawFolderRepoAllowDeletion = value; OnPropertyChanged(); } }
		private bool _rawFolderRepoAllowDeletion = false;
		
		[AlephXMLField(ReconnectRepo=true)]
		public bool RawFolderRepoAllowCreation { get { return _rawFolderRepoAllowCreation; } set { _rawFolderRepoAllowCreation = value; OnPropertyChanged(); } }
		private bool _rawFolderRepoAllowCreation = false;
		
		[AlephXMLField(ReconnectRepo=true)]
		public bool RawFolderRepoAllowModification { get { return _rawFolderRepoAllowModification; } set { _rawFolderRepoAllowModification = value; OnPropertyChanged(); } }
		private bool _rawFolderRepoAllowModification = true;
		
		[AlephXMLField(ReconnectRepo=true)]
		public EncodingEnum RawFolderRepoEncoding { get { return _rawFolderRepoEncoding; } set { _rawFolderRepoEncoding = value; OnPropertyChanged(); } }
		private EncodingEnum _rawFolderRepoEncoding = EncodingEnum.UTF8;
		
		[AlephXMLField(ReconnectRepo=true)]
		public bool RawFolderRepoUseFileWatcher { get { return _rawFolderRepoUseFileWatcher; } set { _rawFolderRepoUseFileWatcher = value; OnPropertyChanged(); } }
		private bool _rawFolderRepoUseFileWatcher = true;
		
		[AlephXMLField(ReconnectRepo=true)]
		public bool RawFolderRepoSubfolders { get { return _rawFolderRepoSubfolders; } set { _rawFolderRepoSubfolders = value; OnPropertyChanged(); } }
		private bool _rawFolderRepoSubfolders = false;
		
		[AlephXMLField(ReconnectRepo=true)]
		public int RawFolderRepoMaxDirectoryDepth { get { return _rawFolderRepoMaxDirectoryDepth; } set { _rawFolderRepoMaxDirectoryDepth = value; OnPropertyChanged(); } }
		private int _rawFolderRepoMaxDirectoryDepth = 5;
		
		[AlephXMLField]
		public bool HideTagChooser { get { return _hideTagChooser; } set { _hideTagChooser = value; OnPropertyChanged(); } }
		private bool _hideTagChooser = false;
		
		[AlephXMLField]
		public bool SuppressConnectionProblemPopup { get { return _suppressConnectionProblemPopup; } set { _suppressConnectionProblemPopup = value; OnPropertyChanged(); } }
		private bool _suppressConnectionProblemPopup = false;
		
		[AlephXMLField]
		public bool SuppressAllSyncProblemsPopup { get { return _suppressAllSyncProblemsPopup; } set { _suppressAllSyncProblemsPopup = value; OnPropertyChanged(); } }
		private bool _suppressAllSyncProblemsPopup = false;
		
		[AlephXMLField]
		public bool UseNaturalNoteSort { get { return _useNaturalNoteSort; } set { _useNaturalNoteSort = value; OnPropertyChanged(); } }
		private bool _useNaturalNoteSort = true;
		
		[AlephXMLField]
		public bool CaseInsensitiveSort { get { return _caseInsensitiveSort; } set { _caseInsensitiveSort = value; OnPropertyChanged(); } }
		private bool _caseInsensitiveSort = true;
		
		[AlephXMLField(IsAdvanced = true)]
		public bool SingleInstanceMode { get { return _singleInstanceMode; } set { _singleInstanceMode = value; OnPropertyChanged(); } }
		private bool _singleInstanceMode = true;

		[AlephXMLField(IsAdvanced = true)]
		public bool AllowAllLettersInFilename { get { return _allowAllLettersInFilename; } set { _allowAllLettersInFilename = value; OnPropertyChanged(); } }
		private bool _allowAllLettersInFilename = false;

		[AlephXMLField(IsAdvanced = true)]
		public bool AllowAllCharactersInFilename { get { return _allowAllCharactersInFilename; } set { _allowAllCharactersInFilename = value; OnPropertyChanged(); } }
		private bool _allowAllCharactersInFilename = false;
		
		[AlephXMLField(IsAdvanced = true)]
		public URLMatchingMode UsedURLMatcher { get { return _usedURLMatcher; } set { _usedURLMatcher = value; OnPropertyChanged(); } }
		private URLMatchingMode _usedURLMatcher = URLMatchingMode.Tolerant;
		int IReadonlyAlephSettings.UsedURLMatchingMode => (int)UsedURLMatcher;

		[AlephXMLField]
		public bool RememberScrollPerSession { get { return _rememberScrollPerSession; } set { _rememberScrollPerSession = value; OnPropertyChanged(); } }
		private bool _rememberScrollPerSession = false;

		[AlephXMLField(IsAdvanced=true)]
		public bool ForceDebugMode { get { return _forceDebugMode; } set { _forceDebugMode = value; OnPropertyChanged(); } }
		private bool _forceDebugMode = false;

		[AlephXMLField(IsAdvanced = true)]
		public bool DisableLogger { get { return _disableLogger; } set { _disableLogger = value; OnPropertyChanged(); } }
		private bool _disableLogger = false;
		
		[AlephXMLField]
		public bool MultiNoteSelection { get { return _multiNoteSelection; } set { _multiNoteSelection = value; OnPropertyChanged(); } }
		private bool _multiNoteSelection = true;
		
		[AlephXMLField]
		public bool VSLineCopy { get { return _vsLineCopy; } set { _vsLineCopy = value; OnPropertyChanged(); } }
		private bool _vsLineCopy = false;

		[AlephXMLField(ReconnectRepo=true)]
		public int NoteDownloadParallelismThreshold { get { return _noteDownloadParallelismThreshold; } set { _noteDownloadParallelismThreshold = value; OnPropertyChanged(); } }
		private int _noteDownloadParallelismThreshold = 10;

		[AlephXMLField(ReconnectRepo=true)]
		public int NoteDownloadParallelismLevel { get { return _noteDownloadParallelismLevel; } set { _noteDownloadParallelismLevel = value; OnPropertyChanged(); } }
		private int _noteDownloadParallelismLevel = 5;

		[AlephXMLField(ReconnectRepo=true)]
		public int NoteNewDownloadParallelismLevel { get { return _noteNewDownloadParallelismLevel; } set { _noteNewDownloadParallelismLevel = value; OnPropertyChanged(); } }
		private int _noteNewDownloadParallelismLevel = 10;

		[AlephXMLField(ReconnectRepo=true)]
		public int NoteNewDownloadParallelismThreshold { get { return _noteNewDownloadParallelismThreshold; } set { _noteNewDownloadParallelismThreshold = value; OnPropertyChanged(); } }
		private int _noteNewDownloadParallelismThreshold = 10;

		[AlephXMLField(ReconnectRepo=true)]
		public int NoteUploadParallelismThreshold { get { return _noteUploadParallelismThreshold; } set { _noteUploadParallelismThreshold = value; OnPropertyChanged(); } }
		private int _noteUploadParallelismThreshold = 999999;

		[AlephXMLField(ReconnectRepo=true)]
		public int NoteUploadParallelismLevel { get { return _noteUploadParallelismLevel; } set { _noteUploadParallelismLevel = value; OnPropertyChanged(); } }
		private int _noteUploadParallelismLevel = 1;
		
		[AlephXMLField]
		public bool ShowTagButton { get { return _showTagButton; } set { _showTagButton = value; OnPropertyChanged(); } }
		private bool _showTagButton = false;

		[AlephXMLField]
		public int SciLineNumberSpacing { get { return _sciLineNumberSpacing; } set { _sciLineNumberSpacing = value; OnPropertyChanged(); } }
		private int _sciLineNumberSpacing = 1;
		
		[AlephXMLField]
		public bool SciHexLineNumber { get { return _sciHexLineNumber; } set { _sciHexLineNumber = value; OnPropertyChanged(); } }
		private bool _sciHexLineNumber = false;

		[AlephXMLField(IsAdvanced = true)]
		public FocusTarget FocusAfterCreateNote { get { return _focusAfterCreateNote; } set { _focusAfterCreateNote = value; OnPropertyChanged(); } }
		private FocusTarget _focusAfterCreateNote = FocusTarget.NoteTitle;
		
		[AlephXMLField(IsAdvanced = true)]
		public bool FocusScintillaOnTitleEnter { get { return _focusScintillaOnTitleEnter; } set { _focusScintillaOnTitleEnter = value; OnPropertyChanged(); } }
		private bool _focusScintillaOnTitleEnter = true;
		
		[AlephXMLField(IsAdvanced = true)]
		public SearchDelayMode GlobalSearchDelay { get { return _globalSearchDelay; } set { _globalSearchDelay = value; OnPropertyChanged(); } }
		private SearchDelayMode _globalSearchDelay = SearchDelayMode.Auto;
		
		[AlephXMLField]
		public bool AutoHideMainMenu { get { return _autoHideMainMenu; } set { _autoHideMainMenu = value; OnPropertyChanged(); } }
		private bool _autoHideMainMenu = false;
		
		[AlephXMLField]
		public bool VerticalMainLayout { get { return _verticalMainLayout; } set { _verticalMainLayout = value; OnPropertyChanged(); } }
		private bool _verticalMainLayout = false;
		
		[AlephXMLField(IsAdvanced = true)]
		public string UIFontFamily { get { return _uiFontFamily; } set { _uiFontFamily = value; OnPropertyChanged(); } }
		private string _uiFontFamily = string.Empty;

		[AlephXMLField]
		public bool SortHierarchyFoldersByName { get { return _sortHierarchyFoldersByName; } set { _sortHierarchyFoldersByName = value; OnPropertyChanged(); } }
		private bool _sortHierarchyFoldersByName = false;

		[AlephXMLField]
		public bool RememberHierarchyExpandedState { get { return _rememberHierarchyExpandedState; } set { _rememberHierarchyExpandedState = value; OnPropertyChanged(); } }
		private bool _rememberHierarchyExpandedState = true;

		[AlephXMLField]
		public bool HideAddNoteButton { get { return _hideAddNoteButton; } set { _hideAddNoteButton = value; OnPropertyChanged(); } }
		private bool _hideAddNoteButton = false;

		[AlephXMLField]
		public bool HideSearchBox { get { return _hideSearchBox; } set { _hideSearchBox = value; OnPropertyChanged(); } }
		private bool _hideSearchBox = false;

		[AlephXMLField(IsAdvanced = true)]
		public bool SyncDownloadOnly { get { return _syncDownloadOnly; } set { _syncDownloadOnly = value; OnPropertyChanged(); } }
		private bool _syncDownloadOnly = false;

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

		public bool GetAnyAdvancedSettingsChanged(out string diff)
        {
			diff = string.Join(";", _serializer.Diff(CreateEmpty(""), this).Where(p => p.Attribute.IsAdvanced).Select(p => p.PropInfo.Name));
			return diff != "";
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

		public static readonly IComparer<INote> COMPARER_PIN_NONE              = ProjectionComparer.CreateExtended<INote, string>(n => n.UniqueName, n => n.IsPinned);
		public static readonly IComparer<INote> COMPARER_PIN_NAME_CS_RAW       = ProjectionComparer.CreateExtendedStr<INote>(n => n.Title, n => n.IsPinned, false, false);
		public static readonly IComparer<INote> COMPARER_PIN_NAME_CS_NATURAL   = ProjectionComparer.CreateExtendedStr<INote>(n => n.Title, n => n.IsPinned, false,  true);
		public static readonly IComparer<INote> COMPARER_PIN_NAME_CI_RAW       = ProjectionComparer.CreateExtendedStr<INote>(n => n.Title.ToLower(), n => n.IsPinned, false, false);
		public static readonly IComparer<INote> COMPARER_PIN_NAME_CI_NATURAL   = ProjectionComparer.CreateExtendedStr<INote>(n => n.Title.ToLower(), n => n.IsPinned, false,  true);
		public static readonly IComparer<INote> COMPARER_PIN_CDATE             = ProjectionComparer.CreateExtended<INote, DateTimeOffset>(n => n.CreationDate, n => n.IsPinned, true);
		public static readonly IComparer<INote> COMPARER_PIN_MDATE             = ProjectionComparer.CreateExtended<INote, DateTimeOffset>(n => n.ModificationDate, n => n.IsPinned, true);
		public static readonly IComparer<INote> COMPARER_NOPIN_NONE            = ProjectionComparer.Create<INote, string>(n => n.UniqueName);
		public static readonly IComparer<INote> COMPARER_NOPIN_NAME_CS_RAW     = ProjectionComparer.CreateStr<INote>(n => n.Title, false, false);
		public static readonly IComparer<INote> COMPARER_NOPIN_NAME_CS_NATURAL = ProjectionComparer.CreateStr<INote>(n => n.Title, false, true);
		public static readonly IComparer<INote> COMPARER_NOPIN_NAME_CI_RAW     = ProjectionComparer.CreateStr<INote>(n => n.Title.ToLower(), false, false);
		public static readonly IComparer<INote> COMPARER_NOPIN_NAME_CI_NATURAL = ProjectionComparer.CreateStr<INote>(n => n.Title.ToLower(), false, true);
		public static readonly IComparer<INote> COMPARER_NOPIN_CDATE           = ProjectionComparer.Create<INote, DateTimeOffset>(n => n.CreationDate, true);
		public static readonly IComparer<INote> COMPARER_NOPIN_MDATE           = ProjectionComparer.Create<INote, DateTimeOffset>(n => n.ModificationDate, true);

		public IComparer<INote> GetNoteComparator()
		{
			switch (NoteSorting)
			{
				case SortingMode.None:
					return SortByPinned ? COMPARER_PIN_NONE : COMPARER_NOPIN_NONE;

				case SortingMode.ByName:
					return SortByPinned
						   ? (UseNaturalNoteSort
						      ? (CaseInsensitiveSort
						         ? COMPARER_PIN_NAME_CI_NATURAL
						         : COMPARER_PIN_NAME_CS_NATURAL)
						      : (CaseInsensitiveSort
						         ? COMPARER_PIN_NAME_CI_RAW
						         : COMPARER_PIN_NAME_CS_RAW))
						   : (UseNaturalNoteSort
						      ? (CaseInsensitiveSort
						         ? COMPARER_NOPIN_NAME_CI_NATURAL
						         : COMPARER_NOPIN_NAME_CS_NATURAL)
						      : (CaseInsensitiveSort
						         ? COMPARER_NOPIN_NAME_CI_RAW
						         : COMPARER_NOPIN_NAME_CS_RAW));

				case SortingMode.ByCreationDate:
					return SortByPinned ? COMPARER_PIN_CDATE : COMPARER_NOPIN_CDATE;

				case SortingMode.ByModificationDate:
					return SortByPinned ? COMPARER_PIN_MDATE : COMPARER_NOPIN_MDATE;

				default: 
					throw new ArgumentOutOfRangeException();
			}
		}

		public HierarchyEmulationConfig GetHierarchicalConfig()
		{
			bool enabled = UseHierarchicalNoteStructure && EmulateHierarchicalStructure;

			var sep = StructureSeperatorHelper.GetSeperator(HStructureSeperator);
			var esc = StructureSeperatorHelper.GetEscapeChar(HStructureSeperator);

			return new HierarchyEmulationConfig(enabled, sep, esc);
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

		public void Migrate(Version from, Version to)
		{
			LoggerSingleton.Inst.Info("AppSettings", $"Migrate settings from {from} to {to}");
			
			_shortcuts = ShortcutManager.MigrateShortcutSettings(from, to, _shortcuts);
		}

		public static List<AXMLFieldInfo> Diff(AppSettings a, AppSettings b)
		{
			return _serializer.Diff(a, b).ToList();
		}

		private static string GetExePath()
		{
			var p = System.Reflection.Assembly.GetExecutingAssembly().Location;
			if (!p.ToLower().EndsWith(".exe")) p = Path.Combine(Path.GetDirectoryName(p) ?? "", "AlephNote.exe");
			return p;
		}

		public bool IsCustomLineNumbers()
		{
			return SciLineNumbers && (SciLineNumberSpacing>1 || SciHexLineNumber);
		}

		public void TriggerReadonlyPropertyChanged()
		{
			OnExplicitPropertyChanged(nameof(IsReadOnlyMode));
		}
	}
}