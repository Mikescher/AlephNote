using AlephNote.PluginInterface;
using MSHC.Math.Encryption;
using MSHC.Util.Helper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace AlephNote.Plugins.StandardNote
{
	public class StandardNoteConfig : IRemoteStorageConfiguration
	{
		private const string ENCRYPTION_KEY = @"HuIpJachKuRyJuOmVelThufCeck";
		
		private const int ID_EMAIL    = 6251;
		private const int ID_PASSWORD = 6252;
		private const int ID_SERVER   = 6253;

		public string Email    = string.Empty;
		public string Password = string.Empty;
		public string Server   = @"https://n3.standardnotes.org";

		public XElement Serialize()
		{
			var data = new object[]
			{
				new XElement("Username", Email),
				new XElement("Password", Encrypt(Password)),
				new XElement("Server", Server),
			};

			var r = new XElement("config", data);
			r.SetAttributeValue("plugin", StandardNotePlugin.Name);
			r.SetAttributeValue("pluginversion", StandardNotePlugin.Version.ToString());
			return r;
		}

		public void Deserialize(XElement input)
		{
			if (input.Name.LocalName != "config") throw new Exception("LocalName != 'config'");

			Email = XHelper.GetChildValue(input, "Email", string.Empty);
			Password = Decrypt(XHelper.GetChildValue(input, "Password", string.Empty));
			Server   = XHelper.GetChildValue(input, "Server", string.Empty);
		}

		public IEnumerable<DynamicSettingValue> ListProperties()
		{
			yield return DynamicSettingValue.CreateText(ID_EMAIL, "Email", Email);
			yield return DynamicSettingValue.CreatePassword(ID_PASSWORD, "Password", Password);
			yield return DynamicSettingValue.CreatePassword(ID_SERVER, "Host", Server);
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
			throw new ArgumentException();
		}

		public bool IsEqual(IRemoteStorageConfiguration iother)
		{
			var other = iother as StandardNoteConfig;
			if (other == null) return false;

			if (this.Email    != other.Email) return false;
			if (this.Password != other.Password) return false;
			if (this.Server   != other.Server) return false;

			return true;
		}

		public IRemoteStorageConfiguration Clone()
		{
			return new StandardNoteConfig
			{
				Email    = this.Email,
				Password = this.Password,
				Server   = this.Server,
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
			return Email + ";" + Server;
		}
	}
}
