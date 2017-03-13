using AlephNote.PluginInterface;
using AlephNote.Plugins;
using AlephNote.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Xml.Linq;

namespace AlephNote.Settings
{
	// ReSharper disable RedundantThisQualifier
	// ReSharper disable CompareOfFloatsByEqualityOperator
	public class AppSettings : ObservableObject
	{
		public const string ENCRYPTION_KEY = @"jcgkZJvoykjpoGkDWHqiNoXoLZRJxpdb";

		private class AttrObj { public PropertyInfo Info; public List<object> Attributes;}

		[Setting]
		public ConfigInterval SynchronizationFrequency { get { return _synchronizationFreq; } set { _synchronizationFreq = value; OnPropertyChanged(); } }
		private ConfigInterval _synchronizationFreq = ConfigInterval.Sync15Min;

		[Setting]
		public bool ProxyEnabled { get { return _proxyEnabled; } set { _proxyEnabled = value; OnPropertyChanged(); } }
		private bool _proxyEnabled = false;

		[Setting]
		public string ProxyHost { get { return _proxyHost; } set { _proxyHost = value; OnPropertyChanged(); } }
		private string _proxyHost = string.Empty;

		[Setting]
		public int? ProxyPort { get { return _proxyPort; } set { _proxyPort = value; OnPropertyChanged(); } }
		private int? _proxyPort = null;

		[Setting]
		public string ProxyUsername { get { return _proxyUsername; } set { _proxyUsername = value; OnPropertyChanged(); } }
		private string _proxyUsername = string.Empty;

		[Setting(Encrypted=true)]
		public string ProxyPassword { get { return _proxyPassword; } set { _proxyPassword = value; OnPropertyChanged(); } }
		private string _proxyPassword = string.Empty;

		[Setting]
		public IRemotePlugin NoteProvider { get { return _noteProvider; } set { _noteProvider = value; OnPropertyChanged(); } }
		private IRemotePlugin _noteProvider = null;

		[Setting]
		public bool MinimizeToTray { get { return _minimizeToTray; } set { _minimizeToTray = value; OnPropertyChanged(); } }
		private bool _minimizeToTray = true;

		[Setting]
		public bool CloseToTray { get { return _closeToTray; } set { _closeToTray = value; OnPropertyChanged(); } }
		private bool _closeToTray = false;

		[Setting]
		public string TitleFontFamily { get { return _titleFontFamily; } set { _titleFontFamily = value; OnPropertyChanged(); } }
		private string _titleFontFamily = string.Empty;

		[Setting]
		public FontModifier TitleFontModifier { get { return _titleFontModifier; } set { _titleFontModifier = value; OnPropertyChanged(); } }
		private FontModifier _titleFontModifier = FontModifier.Bold;

		[Setting]
		public FontSize TitleFontSize { get { return _titleFontSize; } set { _titleFontSize = value; OnPropertyChanged(); } }
		private FontSize _titleFontSize = FontSize.Size16;

		[Setting]
		public string NoteFontFamily { get { return _noteFontFamily; } set { _noteFontFamily = value; OnPropertyChanged(); } }
		private string _noteFontFamily = string.Empty;

		[Setting]
		public FontModifier NoteFontModifier { get { return _noteFontModifier; } set { _noteFontModifier = value; OnPropertyChanged(); } }
		private FontModifier _noteFontModifier = FontModifier.Normal;

		[Setting]
		public FontSize NoteFontSize { get { return _noteFontSize; } set { _noteFontSize = value; OnPropertyChanged(); } }
		private FontSize _noteFontSize = FontSize.Size08;

		[Setting]
		public string ListFontFamily { get { return _listFontFamily; } set { _listFontFamily = value; OnPropertyChanged(); } }
		private string _listFontFamily = string.Empty;

		[Setting]
		public FontModifier ListFontModifier { get { return _listFontModifier; } set { _listFontModifier = value; OnPropertyChanged(); } }
		private FontModifier _listFontModifier = FontModifier.Normal;

		[Setting]
		public FontSize ListFontSize { get { return _listFontSize; } set { _listFontSize = value; OnPropertyChanged(); } }
		private FontSize _listFontSize = FontSize.Size12;

		[Setting]
		public bool SciLineNumbers { get { return _sciLineNumbers; } set { _sciLineNumbers = value; OnPropertyChanged(); } }
		private bool _sciLineNumbers = false;

		[Setting]
		public bool SciRectSelection { get { return _sciRectSelection; } set { _sciRectSelection = value; OnPropertyChanged(); } }
		private bool _sciRectSelection = false;

		[Setting]
		public bool SciZoomable { get { return _sciZoomable; } set { _sciZoomable = value; OnPropertyChanged(); } }
		private bool _sciZoomable = true;

		[Setting]
		public bool SciUseTabs { get { return _sciUseTabs; } set { _sciUseTabs = value; OnPropertyChanged(); } }
		private bool _sciUseTabs = true;
		
		[Setting]
		public bool SciWordWrap { get { return _sciWordWrap; } set { _sciWordWrap = value; OnPropertyChanged(); } }
		private bool _sciWordWrap = false;

		[Setting]
		public bool SciShowWhitespace { get { return _sciShowWhitespace; } set { _sciShowWhitespace = value; OnPropertyChanged(); } }
		private bool _sciShowWhitespace = false;

		[Setting]
		public bool SciShowEOL { get { return _sciShowEOL; } set { _sciShowEOL = value; OnPropertyChanged(); } }
		private bool _sciShowEOL = false;

		[Setting]
		public int SciTabWidth { get { return _sciTabWidth; } set { _sciTabWidth = value; OnPropertyChanged(); } }
		private int _sciTabWidth = 4;

		[Setting]
		public int StartupPositionX { get { return _startupPositionX; } set { _startupPositionX = value; OnPropertyChanged(); } }
		private int _startupPositionX = 64;

		[Setting]
		public int StartupPositionY { get { return _startupPositionY; } set { _startupPositionY = value; OnPropertyChanged(); } }
		private int _startupPositionY = 64;

		[Setting]
		public int StartupPositionWidth { get { return _startupPositionWidth; } set { _startupPositionWidth = value; OnPropertyChanged(); } }
		private int _startupPositionWidth = 525;

		[Setting]
		public int StartupPositionHeight { get { return _startupPositionHeight; } set { _startupPositionHeight = value; OnPropertyChanged(); } }
		private int _startupPositionHeight = 350;

		[Setting]
		public ExtendedWindowStartupLocation StartupLocation { get { return _startupLocation; } set { _startupLocation = value; OnPropertyChanged(); } }
		private ExtendedWindowStartupLocation _startupLocation = ExtendedWindowStartupLocation.ScreenBottomLeft;
		
		[Setting]
		public ExtendedWindowState StartupState { get { return _startupState; } set { _startupState = value; OnPropertyChanged(); } }
		private ExtendedWindowState _startupState = ExtendedWindowState.Normal;

		[Setting]
		public bool LaunchOnBoot { get { return _launchOnBoot; } set { _launchOnBoot = value; OnPropertyChanged(); } }
		private bool _launchOnBoot = false;

		[Setting]
		public SortingMode NoteSorting { get { return _noteSorting; } set { _noteSorting = value; OnPropertyChanged(); } }
		private SortingMode _noteSorting = SortingMode.ByModificationDate;

		[Setting]
		public int SciZoom { get { return _sciZoom; } set { _sciZoom = value; OnPropertyChanged(); } }
		private int _sciZoom = 1;

		[Setting]
		public double OverviewListWidth { get { return _overviewListWidth; } set { _overviewListWidth = value; OnPropertyChanged(); } }
		private double _overviewListWidth = 150;

		[Setting]
		public NotePreviewStyle NotePreviewStyle { get { return _notePreviewStyle; } set { _notePreviewStyle = value; OnPropertyChanged(); } }
		private NotePreviewStyle _notePreviewStyle = NotePreviewStyle.Extended;

		[Setting]
		public ConflictResolutionStrategy ConflictResolution { get { return _conflictResolution; } set { _conflictResolution = value; OnPropertyChanged(); } }
		private ConflictResolutionStrategy _conflictResolution = ConflictResolutionStrategy.UseClientCreateConflictFile;

		public bool CheckForUpdates { get { return _checkForUpdates; } set { _checkForUpdates = value; OnPropertyChanged(); } }
		private bool _checkForUpdates = true;

		public Dictionary<Guid, IRemoteStorageConfiguration> PluginSettings = new Dictionary<Guid, IRemoteStorageConfiguration>();

		private static readonly List<Tuple<SettingType, SettingAttribute, PropertyInfo>> _settingProperties;

		static AppSettings()
		{
			_settingProperties = typeof(AppSettings)
				.GetProperties()
				.Select(p => new AttrObj { Info = p, Attributes = p.GetCustomAttributes(typeof(SettingAttribute), false).Cast<object>().ToList() })
				.Where(p => p.Attributes.Count == 1)
				.Select(p => Tuple.Create(((SettingAttribute)p.Attributes.Single()).GetSettingType(p.Info), (SettingAttribute)p.Attributes.Single(), p.Info))
				.ToList();
		}

		private readonly string _path;

		private AppSettings(string path)
		{
			_path = path;
		}

		public static AppSettings CreateEmpty(string path)
		{
			var r = new AppSettings(path);
			r._noteProvider = PluginManager.GetDefaultPlugin();

			foreach (var plugin in PluginManager.LoadedPlugins)
			{
				if (!r.PluginSettings.ContainsKey(plugin.GetUniqueID()))
					r.PluginSettings[plugin.GetUniqueID()] = plugin.CreateEmptyRemoteStorageConfiguration();
			}

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
			var root = new XElement("configuration");

			foreach (var prop in _settingProperties)
			{
				prop.Item2.Serialize(prop.Item1, prop.Item3, this, root);
			}

			foreach (var setting in PluginSettings)
			{
				var pluginNode = new XElement("Plugin");
				pluginNode.SetAttributeValue("uuid", setting.Key.ToString("B"));
				pluginNode.Add(setting.Value.Serialize());
				root.Add(pluginNode);
			}

			return XHelper.ConvertToString(new XDocument(root));
		}

		public static AppSettings Deserialize(string xml, string path)
		{
			var xd = XDocument.Parse(xml);
			var root = xd.Root;
			if (root == null) throw new Exception("XDocument needs root");
			
			var r = new AppSettings(path);

			foreach (var prop in _settingProperties)
			{
				prop.Item2.Deserialize(prop.Item1, prop.Item3, r, root);
			}
			
			r.PluginSettings = new Dictionary<Guid, IRemoteStorageConfiguration>();

			foreach (var pluginNode in root.Descendants("Plugin"))
			{
				var id = pluginNode.GuidAttribute("uuid");
				var plugin = PluginManager.GetPlugin(id);
				if (plugin != null)
				{
					var cfg = plugin.CreateEmptyRemoteStorageConfiguration();
					cfg.Deserialize(pluginNode.Elements().Single());
					r.PluginSettings[id] = cfg;
				}
			}

			foreach (var plugin in PluginManager.LoadedPlugins)
			{
				if (!r.PluginSettings.ContainsKey(plugin.GetUniqueID()))
					r.PluginSettings[plugin.GetUniqueID()] = plugin.CreateEmptyRemoteStorageConfiguration();
			}

			return r;
		}
		
		public AppSettings Clone()
		{
			var r = new AppSettings(_path);

			foreach (var prop in _settingProperties)
			{
				var v = prop.Item3.GetValue(this);
				prop.Item3.SetValue(r, v);
			}
			
			foreach (var setting in PluginSettings)
			{
				r.PluginSettings[setting.Key] = setting.Value.Clone();
			}

			return r;
		}

		public bool IsEqual(AppSettings other)
		{
			foreach (var prop in _settingProperties)
			{
				if (!prop.Item2.TestEquality(prop.Item1, prop.Item3, this, other)) return false;
			}

			foreach (var key in PluginSettings.Keys.Union(other.PluginSettings.Keys))
			{
				if (!this.PluginSettings.ContainsKey(key)) return false;
				if (!other.PluginSettings.ContainsKey(key)) return false;

				if (!this.PluginSettings[key].IsEqual(other.PluginSettings[key])) return false;
			}

			return true;
		}

		public IWebProxy CreateProxy()
		{
			if (ProxyEnabled)
			{
				if (string.IsNullOrWhiteSpace(ProxyUsername) && string.IsNullOrWhiteSpace(ProxyPassword))
				{
					return new WebProxy(ProxyHost, ProxyPort ?? 443);
				}
				else
				{
					return new WebProxy(ProxyHost, ProxyPort ?? 443)
					{
						Credentials = new NetworkCredential(ProxyUsername, ProxyPassword)
					};
				}
			}
			else
			{
				return new WebProxy();
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
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public IComparer GetNoteComparator()
		{
			switch (NoteSorting)
			{
				case SortingMode.None:
					return ProjectionComparer.Create<INote, string>(n => n.GetUniqueName());
				case SortingMode.ByName:
					return ProjectionComparer.Create<INote, string>(n => n.Title);
				case SortingMode.ByCreationDate:
					return ProjectionComparer.Create<INote, DateTimeOffset>(n => n.CreationDate, true);
				case SortingMode.ByModificationDate:
					return ProjectionComparer.Create<INote, DateTimeOffset>(n => n.ModificationDate, true);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
