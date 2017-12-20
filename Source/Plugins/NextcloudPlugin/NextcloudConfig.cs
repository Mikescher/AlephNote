using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Plugins.Nextcloud
{
	public class NextcloudConfig : IRemoteStorageConfiguration
	{
		private const string ENCRYPTION_KEY = @"SenChkLv443FmqXXiD3wRh9srMuGpYdi"; // https://duckduckgo.com/?q=random+password+32+characters
		
		private const int ID_USERNAME = 6351;
		private const int ID_PASSWORD = 6352;
		private const int ID_HOST     = 6353;
		private const int ID_BLANKFMT = 6354;

		public string Username = string.Empty;
		public string Password = string.Empty;
		public string Host = "https://example.com";
		public bool BlankLineBelowTitle = true;

		public XElement Serialize()
		{
			var data = new object[]
			{
				new XElement("Username", Username),
				new XElement("Password", Encrypt(Password)),
				new XElement("Host", Host),
				new XElement("BlankLineBelowTitle", BlankLineBelowTitle),
			};

			var r = new XElement("config", data);
			r.SetAttributeValue("plugin", NextcloudPlugin.Name);
			r.SetAttributeValue("pluginversion", NextcloudPlugin.Version.ToString());
			return r;
		}

		public void Deserialize(XElement input)
		{
			if (input.Name.LocalName != "config") throw new Exception("LocalName != 'config'");

			Username = XHelper.GetChildValue(input, "Username", Username);
			Password = Decrypt(XHelper.GetChildValue(input, "Password", string.Empty));
			Host = XHelper.GetChildValue(input, "Host", Host);
		}

		public IEnumerable<DynamicSettingValue> ListProperties()
		{
			yield return DynamicSettingValue.CreateText(ID_USERNAME, "Username", Username);
			yield return DynamicSettingValue.CreatePassword(ID_PASSWORD, "Password", Password);
			yield return DynamicSettingValue.CreateText(ID_HOST, "Server address", Host);
			yield return DynamicSettingValue.CreateCheckbox(ID_BLANKFMT, "Empty line between title and content", BlankLineBelowTitle);
		}

		public void SetProperty(int id, string value)
		{
			if (id == ID_USERNAME) Username = value;
			if (id == ID_PASSWORD) Password = value;
			if (id == ID_HOST) Host = value;
		}

		public void SetProperty(int id, bool value)
		{
			if (id == ID_BLANKFMT) BlankLineBelowTitle = value;
		}

		public void SetProperty(int id, int value)
		{
			throw new NotSupportedException();
		}

		public bool IsEqual(IRemoteStorageConfiguration iother)
		{
			var other = iother as NextcloudConfig;
			if (other == null) return false;

			if (this.Username            != other.Username) return false;
			if (this.Password            != other.Password) return false;
			if (this.Host                != other.Host) return false;
			if (this.BlankLineBelowTitle != other.BlankLineBelowTitle) return false;

			return true;
		}

		public IRemoteStorageConfiguration Clone()
		{
			return new NextcloudConfig
			{
				Username            = this.Username,
				Password            = this.Password,
				Host                = this.Host,
				BlankLineBelowTitle = this.BlankLineBelowTitle,
			};
		}

		private string Encrypt(string data)
		{
			if (string.IsNullOrWhiteSpace(data)) return string.Empty;
			return Convert.ToBase64String(AESThenHMAC.SimpleEncryptWithPassword(Encoding.UTF32.GetBytes(data), ENCRYPTION_KEY));
		}

		private string Decrypt(string data)
		{
			if (string.IsNullOrWhiteSpace(data)) return string.Empty;
			return Encoding.UTF32.GetString(AESThenHMAC.SimpleDecryptWithPassword(Convert.FromBase64String(data), ENCRYPTION_KEY));
		}

		public string GetDisplayIdentifier()
		{
			return Username + "@" + Host;
		}
	}
}
