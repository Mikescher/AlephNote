using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Plugins.StandardNote
{
	public class StandardNoteConfig : IRemoteStorageConfiguration
	{
		private const string ENCRYPTION_KEY = @"HuIpJachKuRyJuOmVelThufCeck"; // https://duckduckgo.com/?q=random+password+32+characters
		
		private const int ID_EMAIL    = 6251;
		private const int ID_PASSWORD = 6252;
		private const int ID_SERVER   = 6253;
		private const int ID_ENCRYPT  = 6254;
		private const int ID_REMTAGS  = 6255;

		public string Email       = string.Empty;
		public string Password    = string.Empty;
		public string Server      = @"https://n3.standardnotes.org";
		public bool SendEncrypted = true;
		public bool RemEmptyTags  = true;

		public XElement Serialize()
		{
			var data = new object[]
			{
				new XElement("Email", Email),
				new XElement("Password", Encrypt(Password)),
				new XElement("Server", Server),
				new XElement("Encrypt", SendEncrypted),
				new XElement("RemEmptyTags", RemEmptyTags),
			};

			var r = new XElement("config", data);
			r.SetAttributeValue("plugin", StandardNotePlugin.Name);
			r.SetAttributeValue("pluginversion", StandardNotePlugin.Version.ToString());
			return r;
		}

		public void Deserialize(XElement input)
		{
			if (input.Name.LocalName != "config") throw new Exception("LocalName != 'config'");

			Email = XHelper.GetChildValue(input, "Email", Email);
			Password = Decrypt(XHelper.GetChildValue(input, "Password", string.Empty));
			Server = XHelper.GetChildValue(input, "Server", Server);
			SendEncrypted = XHelper.GetChildValue(input, "Encrypt", SendEncrypted);
			RemEmptyTags = XHelper.GetChildValue(input, "RemEmptyTags", RemEmptyTags);
		}

		public IEnumerable<DynamicSettingValue> ListProperties()
		{
			yield return DynamicSettingValue.CreateText(ID_EMAIL, "Email", Email);
			yield return DynamicSettingValue.CreatePassword(ID_PASSWORD, "Password", Password);
			yield return DynamicSettingValue.CreateText(ID_SERVER, "Host", Server);
			yield return DynamicSettingValue.CreateCheckbox(ID_ENCRYPT, "Encrypt Notes", SendEncrypted);
			yield return DynamicSettingValue.CreateCheckbox(ID_REMTAGS, "Delete unused tags", RemEmptyTags);
			yield return DynamicSettingValue.CreateHyperlink("Create Standard Notes account", "https://standardnotes.org/");
		}

		public void SetProperty(int id, string value)
		{
			if (id == ID_EMAIL) Email = value;
			if (id == ID_PASSWORD) Password = value;
			if (id == ID_SERVER) Server = value;
		}

		public void SetProperty(int id, bool value)
		{
			if (id == ID_ENCRYPT) SendEncrypted = value;
			if (id == ID_REMTAGS) RemEmptyTags = value;
		}

		public bool IsEqual(IRemoteStorageConfiguration iother)
		{
			var other = iother as StandardNoteConfig;
			if (other == null) return false;

			if (this.Email         != other.Email) return false;
			if (this.Password      != other.Password) return false;
			if (this.Server        != other.Server) return false;
			if (this.SendEncrypted != other.SendEncrypted) return false;
			if (this.RemEmptyTags  != other.RemEmptyTags) return false;

			return true;
		}

		public IRemoteStorageConfiguration Clone()
		{
			return new StandardNoteConfig
			{
				Email         = this.Email,
				Password      = this.Password,
				Server        = this.Server,
				SendEncrypted = this.SendEncrypted,
				RemEmptyTags  = this.RemEmptyTags,
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
			return Email;
		}
	}
}
