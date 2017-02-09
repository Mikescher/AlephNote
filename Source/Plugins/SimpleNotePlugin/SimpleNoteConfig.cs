using CommonNote.PluginInterface;
using MSHC.Math.Encryption;
using MSHC.Util.Helper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace CommonNote.Plugins.SimpleNote
{
	public class SimpleNoteConfig : IRemoteStorageConfiguration
	{
		private const string ENCRYPTION_KEY = @"rLPDWseNePtqLjXuYRdAAWjQnJoSjxjp";
		
		private const int ID_USERNAME = 6151;
		private const int ID_PASSWORD = 6152;

		public string SimpleNoteUsername = string.Empty;
		public string SimpleNotePassword = string.Empty;

		public XElement Serialize()
		{
			var data = new object[]
			{
				new XElement("SimpleNoteUsername", SimpleNoteUsername),
				new XElement("SimpleNotePassword", Encrypt(SimpleNotePassword)),
			};

			var r = new XElement("config", data);
			r.SetAttributeValue("plugin", "SimpleNotePlugin");
			r.SetAttributeValue("pluginversion", SimpleNotePlugin.Version.ToString());
			return r;
		}

		public void Deserialize(XElement input)
		{
			if (input.Name.LocalName != "config") throw new Exception("LocalName != 'config'");

			SimpleNoteUsername = XHelper.GetChildValue(input, "SimpleNoteUsername", string.Empty);
			SimpleNotePassword = Decrypt(XHelper.GetChildValue(input, "SimpleNotePassword", string.Empty));
		}

		public IEnumerable<DynamicSettingValue> ListProperties()
		{
			yield return DynamicSettingValue.CreateText(ID_USERNAME, "Username", SimpleNoteUsername);
			yield return DynamicSettingValue.CreatePassword(ID_PASSWORD, "Password", SimpleNotePassword);
		}

		public void SetProperty(int id, string value)
		{
			switch (id)
			{
				case ID_USERNAME:
					SimpleNoteUsername = value;
					return;
				case ID_PASSWORD:
					SimpleNotePassword = value;
					return;
				default:
					throw new Exception("Invalid PropertyID: " + id);
			}
		}

		public bool IsEqual(IRemoteStorageConfiguration iother)
		{
			var other = iother as SimpleNoteConfig;
			if (other == null) return false;

			if (this.SimpleNoteUsername != other.SimpleNoteUsername) return false;
			if (this.SimpleNotePassword != other.SimpleNotePassword) return false;

			return true;
		}

		public IRemoteStorageConfiguration Clone()
		{
			return new SimpleNoteConfig
			{
				SimpleNoteUsername = this.SimpleNoteUsername,
				SimpleNotePassword = this.SimpleNotePassword,
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

		public string GetUniqueName()
		{
			return SimpleNoteUsername;
		}
	}
}
