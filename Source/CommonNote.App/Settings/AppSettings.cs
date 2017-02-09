using CommonNote.PluginInterface;
using MSHC.Lang.Extensions;
using MSHC.Math.Encryption;
using MSHC.Util.Helper;
using MSHC.WPF.MVVM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CommonNote.Settings
{
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
			root.Add(new XElement("ProxyEnabled", ProxyEnabled));
			root.Add(new XElement("ProxyHost", ProxyHost));
			root.Add(new XElement("ProxyPort", ProxyPort == null ? "" : ProxyPort.ToString()));
			root.Add(new XElement("ProxyUsername", ProxyUsername));
			root.Add(new XElement("ProxyPassword", Encrypt(ProxyPassword)));
			root.Add(new XElement("NoteProvider", NoteProvider.GetUniqueID().ToString("B")));

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

			r.SynchronizationFrequency = XHelper.GetChildValue(root, "SynchronizationFrequency", ConfigInterval.Sync15Min);
			r.ProxyEnabled = XHelper.GetChildValue(root, "ProxyEnabled", false);
			r.ProxyHost = XHelper.GetChildValue(root, "ProxyHost", string.Empty);
			r.ProxyPort = XHelper.GetChildValue(root, "ProxyPort", (int?)null);
			r.ProxyUsername = XHelper.GetChildValue(root, "ProxyUsername", string.Empty);
			r.ProxyPassword = Decrypt(XHelper.GetChildValue(root, "ProxyPassword", string.Empty));
			r.NoteProvider = PluginManager.GetPlugin(XHelper.GetChildValue(root, "NoteProvider", PluginManager.GetDefaultPlugin().GetUniqueID()));

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
			r.ProxyEnabled = this.ProxyEnabled;
			r.ProxyHost = this.ProxyHost;
			r.ProxyPort = this.ProxyPort;
			r.ProxyUsername = this.ProxyUsername;
			r.ProxyPassword = this.ProxyPassword;
			r.NoteProvider = this.NoteProvider;

			foreach (var setting in PluginSettings)
			{
				r.PluginSettings[setting.Key] = setting.Value.Clone();
			}

			return r;
		}

		public bool IsEqual(AppSettings other)
		{
			if (this.SynchronizationFrequency != other.SynchronizationFrequency) return false;
			if (this.ProxyEnabled != other.ProxyEnabled) return false;
			if (this.ProxyHost != other.ProxyHost) return false;
			if (this.ProxyPort != other.ProxyPort) return false;
			if (this.ProxyUsername != other.ProxyUsername) return false;
			if (this.ProxyPassword != other.ProxyPassword) return false;
			if (this.NoteProvider != other.NoteProvider) return false;

			foreach (var key in PluginSettings.Keys.Union(other.PluginSettings.Keys))
			{
				if (!this.PluginSettings.ContainsKey(key)) return false;
				if (!other.PluginSettings.ContainsKey(key)) return false;

				if (!this.PluginSettings[key].IsEqual(other.PluginSettings[key])) return false;
			}

			return true;
		}
	}
}
