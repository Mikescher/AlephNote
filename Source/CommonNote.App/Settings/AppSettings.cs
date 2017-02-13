using CommonNote.PluginInterface;
using MSHC.Lang.Extensions;
using MSHC.Math.Encryption;
using MSHC.Util.Helper;
using MSHC.WPF.MVVM;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Xml.Linq;

namespace CommonNote.Settings
{
	// ReSharper disable RedundantThisQualifier
	// ReSharper disable CompareOfFloatsByEqualityOperator
	public class AppSettings : ObservableObject
	{
		private const string ENCRYPTION_KEY = @"jcgkZJvoykjpoGkDWHqiNoXoLZRJxpdb";

		private ConfigInterval _synchronizationFreq = ConfigInterval.Sync15Min;
		public ConfigInterval SynchronizationFrequency { get { return _synchronizationFreq; } set { _synchronizationFreq = value; OnPropertyChanged(); } }

		private bool _proxyEnabled = false;
		public bool ProxyEnabled { get { return _proxyEnabled; } set { _proxyEnabled = value; OnPropertyChanged(); } }

		private string _proxyHost = string.Empty;
		public string ProxyHost { get { return _proxyHost; } set { _proxyHost = value; OnPropertyChanged(); } }

		private int? _proxyPort = null;
		public int? ProxyPort { get { return _proxyPort; } set { _proxyPort = value; OnPropertyChanged(); } }

		private string _proxyUsername = string.Empty;
		public string ProxyUsername { get { return _proxyUsername; } set { _proxyUsername = value; OnPropertyChanged(); } }

		private string _proxyPassword = string.Empty;
		public string ProxyPassword { get { return _proxyPassword; } set { _proxyPassword = value; OnPropertyChanged(); } }

		private IRemoteProvider _noteProvider = null;
		public IRemoteProvider NoteProvider { get { return _noteProvider; } set { _noteProvider = value; OnPropertyChanged(); } }

		private bool _minimizeToTray = true;
		public bool MinimizeToTray { get { return _minimizeToTray; } set { _minimizeToTray = value; OnPropertyChanged(); } }

		private bool _closeToTray = false;
		public bool CloseToTray { get { return _closeToTray; } set { _closeToTray = value; OnPropertyChanged(); } }

		private FontFamily _titleFontName = FontFamily.Families.FirstOrDefault(p => p.Name == "Segoe UI") ?? FontFamily.GenericSansSerif;
		public FontFamily TitleFontName { get { return _titleFontName; } set { _titleFontName = value; OnPropertyChanged(); } }

		private FontModifier _titleFontModifier = FontModifier.Bold;
		public FontModifier TitleFontModifier { get { return _titleFontModifier; } set { _titleFontModifier = value; OnPropertyChanged(); } }

		private FontSize _titleFontSize = FontSize.Size16;
		public FontSize TitleFontSize { get { return _titleFontSize; } set { _titleFontSize = value; OnPropertyChanged(); } }

		private FontFamily _noteFontName = FontFamily.Families.FirstOrDefault(p => p.Name == "Segoe UI") ?? FontFamily.GenericSansSerif;
		public FontFamily NoteFontName { get { return _noteFontName; } set { _noteFontName = value; OnPropertyChanged(); } }

		private FontModifier _noteFontModifier = FontModifier.Normal;
		public FontModifier NoteFontModifier { get { return _noteFontModifier; } set { _noteFontModifier = value; OnPropertyChanged(); } }

		private FontSize _noteFontSize = FontSize.Size08;
		public FontSize NoteFontSize { get { return _noteFontSize; } set { _noteFontSize = value; OnPropertyChanged(); } }

		private FontFamily _listFontName = FontFamily.Families.FirstOrDefault(p => p.Name == "Segoe UI") ?? FontFamily.GenericSansSerif;
		public FontFamily ListFontName { get { return _listFontName; } set { _listFontName = value; OnPropertyChanged(); } }

		private FontModifier _listFontModifier = FontModifier.Normal;
		public FontModifier ListFontModifier { get { return _listFontModifier; } set { _listFontModifier = value; OnPropertyChanged(); } }

		private FontSize _listFontSize = FontSize.Size12;
		public FontSize ListFontSize { get { return _listFontSize; } set { _listFontSize = value; OnPropertyChanged(); } }

		private bool _sciLineNumbers = false;
		public bool SciLineNumbers { get { return _sciLineNumbers; } set { _sciLineNumbers = value; OnPropertyChanged(); } }

		private bool _sciRectSelection = false;
		public bool SciRectSelection { get { return _sciRectSelection; } set { _sciRectSelection = value; OnPropertyChanged(); } }

		private bool _sciWordWrap = false;
		public bool SciWordWrap { get { return _sciWordWrap; } set { _sciWordWrap = value; OnPropertyChanged(); } }

		private bool _sciShowWhitespace = false;
		public bool SciShowWhitespace { get { return _sciShowWhitespace; } set { _sciShowWhitespace = value; OnPropertyChanged(); } }

		private bool _sciShowEOL = false;
		public bool SciShowEOL { get { return _sciShowEOL; } set { _sciShowEOL = value; OnPropertyChanged(); } }

		private int _startupPositionX = 64;
		public int StartupPositionX { get { return _startupPositionX; } set { _startupPositionX = value; OnPropertyChanged(); } }

		private int _startupPositionY = 64;
		public int StartupPositionY { get { return _startupPositionY; } set { _startupPositionY = value; OnPropertyChanged(); } }

		private int _startupPositionWidth = 350;
		public int StartupPositionWidth { get { return _startupPositionWidth; } set { _startupPositionWidth = value; OnPropertyChanged(); } }

		private int _startupPositionHeight = 525;
		public int StartupPositionHeight { get { return _startupPositionHeight; } set { _startupPositionHeight = value; OnPropertyChanged(); } }

		private WindowStartupLocation _startupLocation = WindowStartupLocation.CenterScreen;
		public WindowStartupLocation StartupLocation { get { return _startupLocation; } set { _startupLocation = value; OnPropertyChanged(); } }

		private WindowState _startupState = WindowState.Normal;
		public WindowState StartupState { get { return _startupState; } set { _startupState = value; OnPropertyChanged(); } }

		public Dictionary<Guid, IRemoteStorageConfiguration> PluginSettings = new Dictionary<Guid, IRemoteStorageConfiguration>(); 

		private AppSettings()
		{
			
		}

		public static AppSettings CreateEmpty()
		{
			var r = new AppSettings();
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
			File.WriteAllText(App.PATH_SETTINGS, Serialize());
		}

		public static AppSettings Load()
		{
			return Deserialize(File.ReadAllText(App.PATH_SETTINGS));
		}

		public string Serialize()
		{
			var root = new XElement("configuration");

			root.Add(new XElement("SynchronizationFrequency", SynchronizationFrequency));
			root.Add(new XElement("ProxyEnabled",             ProxyEnabled));
			root.Add(new XElement("ProxyHost",                ProxyHost));
			root.Add(new XElement("ProxyPort",                ProxyPort == null ? "" : ProxyPort.ToString()));
			root.Add(new XElement("ProxyUsername",            ProxyUsername));
			root.Add(new XElement("ProxyPassword",            Encrypt(ProxyPassword)));
			root.Add(new XElement("NoteProvider",             NoteProvider.GetUniqueID().ToString("B")));
			root.Add(new XElement("CloseToTray",              CloseToTray));
			root.Add(new XElement("MinimizeToTray",           MinimizeToTray));

			root.Add(new XElement("TitleFontName",            TitleFontName.Name));
			root.Add(new XElement("TitleFontModifier",        TitleFontModifier));
			root.Add(new XElement("TitleFontSize",            TitleFontSize));
			root.Add(new XElement("NoteFontName",             NoteFontName.Name));
			root.Add(new XElement("NoteFontModifier",         NoteFontModifier));
			root.Add(new XElement("NoteFontSize",             NoteFontSize));
			root.Add(new XElement("ListFontName",             ListFontName.Name));
			root.Add(new XElement("ListFontModifier",         ListFontModifier));
			root.Add(new XElement("ListFontSize",             ListFontSize));

			root.Add(new XElement("SciLineNumbers",           SciLineNumbers));
			root.Add(new XElement("SciRectSelection",         SciRectSelection));
			root.Add(new XElement("SciWordWrap",              SciWordWrap));
			root.Add(new XElement("SciShowWhitespace",        SciShowWhitespace));
			root.Add(new XElement("SciShowEOL",               SciShowEOL));

			root.Add(new XElement("StartupLocation",          StartupLocation));
			root.Add(new XElement("StartupState",             StartupState));
			root.Add(new XElement("StartupPositionX",         StartupPositionX));
			root.Add(new XElement("StartupPositionY",         StartupPositionY));
			root.Add(new XElement("StartupPositionWidth",     StartupPositionWidth));
			root.Add(new XElement("StartupPositionHeight",    StartupPositionHeight));

			foreach (var setting in PluginSettings)
			{
				var pluginNode = new XElement("Plugin");
				pluginNode.SetAttributeValue("uuid", setting.Key.ToString("B"));
				pluginNode.Add(setting.Value.Serialize());
				root.Add(pluginNode);
			}

			return XHelper.ConvertToString(new XDocument(root));
		}

		public static AppSettings Deserialize(string xml)
		{
			var xd = XDocument.Parse(xml);
			var root = xd.Root;
			if (root == null) throw new Exception("XDocument needs root");
			
			var r = new AppSettings();

			r.SynchronizationFrequency = XHelper.GetChildValue(root, "SynchronizationFrequency", r.SynchronizationFrequency);
			r.ProxyEnabled             = XHelper.GetChildValue(root, "ProxyEnabled", r.ProxyEnabled);
			r.ProxyHost                = XHelper.GetChildValue(root, "ProxyHost", r.ProxyHost);
			r.ProxyPort                = XHelper.GetChildValue(root, "ProxyPort", r.ProxyPort);
			r.ProxyUsername            = XHelper.GetChildValue(root, "ProxyUsername", r.ProxyUsername);
			r.ProxyPassword            = Decrypt(XHelper.GetChildValue(root, "ProxyPassword", string.Empty));
			r.NoteProvider             = PluginManager.GetPlugin(XHelper.GetChildValue(root, "NoteProvider", PluginManager.GetDefaultPlugin().GetUniqueID()));
			r.MinimizeToTray           = XHelper.GetChildValue(root, "MinimizeToTray", r.MinimizeToTray);
			r.CloseToTray              = XHelper.GetChildValue(root, "CloseToTray", r.CloseToTray);

			r.TitleFontName            = GetFontByNameOrDefault(XHelper.GetChildValue(root, "TitleFontName", r.TitleFontName.Name), r.TitleFontName);
			r.TitleFontModifier        = XHelper.GetChildValue(root, "TitleFontModifier", r.TitleFontModifier);
			r.TitleFontSize            = XHelper.GetChildValue(root, "TitleFontSize", r.TitleFontSize);
			r.NoteFontName             = GetFontByNameOrDefault(XHelper.GetChildValue(root, "NoteFontName", r.NoteFontName.Name), r.NoteFontName);
			r.NoteFontModifier         = XHelper.GetChildValue(root, "NoteFontModifier", r.NoteFontModifier);
			r.NoteFontSize             = XHelper.GetChildValue(root, "NoteFontSize", r.NoteFontSize);
			r.ListFontName             = GetFontByNameOrDefault(XHelper.GetChildValue(root, "ListFontName", r.ListFontName.Name), r.ListFontName);
			r.ListFontModifier         = XHelper.GetChildValue(root, "ListFontModifier", r.ListFontModifier);
			r.ListFontSize             = XHelper.GetChildValue(root, "ListFontSize", r.ListFontSize);

			r.SciLineNumbers           = XHelper.GetChildValue(root, "SciLineNumbers", r.SciLineNumbers);
			r.SciRectSelection         = XHelper.GetChildValue(root, "SciRectSelection", r.SciRectSelection);
			r.SciWordWrap              = XHelper.GetChildValue(root, "SciWordWrap", r.SciWordWrap);
			r.SciShowWhitespace        = XHelper.GetChildValue(root, "SciShowWhitespace", r.SciShowWhitespace);
			r.SciShowEOL               = XHelper.GetChildValue(root, "SciShowEOL", r.SciShowEOL);

			r.StartupLocation          = XHelper.GetChildValue(root, "StartupLocation", r.StartupLocation);
			r.StartupState             = XHelper.GetChildValue(root, "StartupState", r.StartupState);
			r.StartupPositionX         = XHelper.GetChildValue(root, "StartupPositionX", r.StartupPositionX);
			r.StartupPositionY         = XHelper.GetChildValue(root, "StartupPositionY", r.StartupPositionY);
			r.StartupPositionWidth     = XHelper.GetChildValue(root, "StartupPositionWidth", r.StartupPositionWidth);
			r.StartupPositionHeight    = XHelper.GetChildValue(root, "StartupPositionHeight", r.StartupPositionHeight);


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

		private static FontFamily GetFontByNameOrDefault(string name, FontFamily defaultFamily)
		{
			return FontFamily.Families.FirstOrDefault(p => p.Name == name) ?? defaultFamily;
		}

		private static string Encrypt(string data)
		{
			if (string.IsNullOrWhiteSpace(data)) return string.Empty;

			return Convert.ToBase64String(AESThenHMAC.SimpleEncryptWithPassword(Encoding.UTF32.GetBytes(data), ENCRYPTION_KEY));
		}

		private static string Decrypt(string data)
		{
			if (string.IsNullOrWhiteSpace(data)) return string.Empty;

			return Encoding.UTF32.GetString(AESThenHMAC.SimpleDecryptWithPassword(Convert.FromBase64String(data), ENCRYPTION_KEY));
		}

		public AppSettings Clone()
		{
			var r = new AppSettings();
			r.SynchronizationFrequency = this.SynchronizationFrequency;
			r.ProxyEnabled             = this.ProxyEnabled;
			r.ProxyHost                = this.ProxyHost;
			r.ProxyPort                = this.ProxyPort;
			r.ProxyUsername            = this.ProxyUsername;
			r.ProxyPassword            = this.ProxyPassword;
			r.NoteProvider             = this.NoteProvider;
			r.MinimizeToTray           = this.MinimizeToTray;
			r.CloseToTray              = this.CloseToTray;

			r.TitleFontName            = this.TitleFontName;
			r.TitleFontModifier        = this.TitleFontModifier;
			r.TitleFontSize            = this.TitleFontSize;
			r.NoteFontName             = this.NoteFontName;
			r.NoteFontModifier         = this.NoteFontModifier;
			r.NoteFontSize             = this.NoteFontSize;
			r.ListFontName             = this.ListFontName;
			r.ListFontModifier         = this.ListFontModifier;
			r.ListFontSize             = this.ListFontSize;

			r.SciLineNumbers           = this.SciLineNumbers;
			r.SciRectSelection         = this.SciRectSelection;
			r.SciWordWrap              = this.SciWordWrap;
			r.SciShowWhitespace        = this.SciShowWhitespace;
			r.SciShowEOL               = this.SciShowEOL;

			r.StartupLocation          = this.StartupLocation;
			r.StartupState             = this.StartupState;
			r.StartupPositionX         = this.StartupPositionX;
			r.StartupPositionY         = this.StartupPositionY;
			r.StartupPositionWidth     = this.StartupPositionWidth;
			r.StartupPositionHeight    = this.StartupPositionHeight;

			foreach (var setting in PluginSettings)
			{
				r.PluginSettings[setting.Key] = setting.Value.Clone();
			}

			return r;
		}

		public bool IsEqual(AppSettings other)
		{
			if (this.SynchronizationFrequency != other.SynchronizationFrequency) return false;
			if (this.ProxyEnabled             != other.ProxyEnabled)             return false;
			if (this.ProxyHost                != other.ProxyHost)                return false;
			if (this.ProxyPort                != other.ProxyPort)                return false;
			if (this.ProxyUsername            != other.ProxyUsername)            return false;
			if (this.ProxyPassword            != other.ProxyPassword)            return false;
			if (this.NoteProvider             != other.NoteProvider)             return false;
			if (this.CloseToTray              != other.CloseToTray)              return false;
			if (this.MinimizeToTray           != other.MinimizeToTray)           return false;
			if (this.MinimizeToTray           != other.MinimizeToTray)           return false;

			if (this.TitleFontName.Name       != other.TitleFontName.Name)       return false;
			if (this.TitleFontModifier        != other.TitleFontModifier)        return false;
			if (this.TitleFontSize            != other.TitleFontSize)            return false;
			if (this.NoteFontName.Name        != other.NoteFontName.Name)        return false;
			if (this.NoteFontModifier         != other.NoteFontModifier)         return false;
			if (this.NoteFontSize             != other.NoteFontSize)             return false;
			if (this.ListFontName.Name        != other.ListFontName.Name)        return false;
			if (this.ListFontModifier         != other.ListFontModifier)         return false;
			if (this.ListFontSize             != other.ListFontSize)             return false;

			if (this.SciLineNumbers           != other.SciLineNumbers)           return false;
			if (this.SciRectSelection         != other.SciRectSelection)         return false;
			if (this.SciWordWrap              != other.SciWordWrap)              return false;
			if (this.SciShowWhitespace        != other.SciShowWhitespace)        return false;
			if (this.SciShowEOL               != other.SciShowEOL)               return false;

			if (this.StartupLocation          != other.StartupLocation)          return false;
			if (this.StartupState             != other.StartupState)             return false;
			if (this.StartupPositionX         != other.StartupPositionX)         return false;
			if (this.StartupPositionY         != other.StartupPositionY)         return false;
			if (this.StartupPositionWidth     != other.StartupPositionWidth)     return false;
			if (this.StartupPositionHeight    != other.StartupPositionHeight)    return false;

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
	}
}
