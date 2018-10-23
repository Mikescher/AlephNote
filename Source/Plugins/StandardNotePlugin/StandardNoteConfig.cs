using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using AlephNote.PluginInterface.Objects;
using AlephNote.PluginInterface.Objects.AXML;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Plugins.StandardNote
{
	public class StandardNoteConfig : IRemoteStorageConfiguration
	{
		private const string ENCRYPTION_KEY = @"HuIpJachKuRyJuOmVelThufCeck"; // https://duckduckgo.com/?q=random+password+32+characters

		public static readonly Regex REX_LINEBREAK = new Regex(@"\r?\n", RegexOptions.Compiled);

		private const int ID_EMAIL    = 6251;
		private const int ID_PASSWORD = 6252;
		private const int ID_SERVER   = 6253;
		private const int ID_REMTAGS  = 6255;

		public string Email       = string.Empty;
		public string Password    = string.Empty;
		public string Server      = @"https://sync.standardnotes.org";
		public bool RemEmptyTags  = true;

		public XElement Serialize(AXMLSerializationSettings opt)
		{
			var data = new object[]
			{
				new XElement("Email", Email),
				new XElement("Password", Encrypt(Password, opt)),
				new XElement("Server", Server),
				new XElement("RemEmptyTags", RemEmptyTags),
			};

			var r = new XElement("config", data);
			r.SetAttributeValue("plugin", StandardNotePlugin.Name);
			r.SetAttributeValue("pluginversion", StandardNotePlugin.Version.ToString());
			return r;
		}

		public void Deserialize(XElement input, AXMLSerializationSettings opt)
		{
			if (input.Name.LocalName != "config") throw new Exception("LocalName != 'config'");

			Email = XHelper.GetChildValue(input, "Email", Email);
			Password = Decrypt(XHelper.GetChildValue(input, "Password", string.Empty), opt);
			Server = XHelper.GetChildValue(input, "Server", Server);
			RemEmptyTags = XHelper.GetChildValue(input, "RemEmptyTags", RemEmptyTags);
		}

		public IEnumerable<DynamicSettingValue> ListProperties()
		{
			yield return DynamicSettingValue.CreateText(ID_EMAIL, "Email", Email);
			yield return DynamicSettingValue.CreatePassword(ID_PASSWORD, "Password", Password);
			yield return DynamicSettingValue.CreateText(ID_SERVER, "Host", Server);
			yield return DynamicSettingValue.CreateCheckbox(ID_REMTAGS, "Delete unused tags", RemEmptyTags, "RemEmptyTags");
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
			if (id == ID_REMTAGS) RemEmptyTags = value;
		}

		public void SetProperty(int id, int value)
		{
			throw new NotImplementedException();
		}

		public bool IsEqual(IRemoteStorageConfiguration iother)
		{
			var other = iother as StandardNoteConfig;
			if (other == null) return false;

			if (this.Email         != other.Email)         return false;
			if (this.Password      != other.Password)      return false;
			if (this.Server        != other.Server)        return false;
			if (this.RemEmptyTags  != other.RemEmptyTags)  return false;

			return true;
		}

		public IRemoteStorageConfiguration Clone()
		{
			return new StandardNoteConfig
			{
				Email         = this.Email,
				Password      = this.Password,
				Server        = this.Server,
				RemEmptyTags  = this.RemEmptyTags,
			};
		}

		private string Encrypt(string data, AXMLSerializationSettings opt)
		{
			return AESThenHMAC.SimpleEncryptWithPassword(data, ENCRYPTION_KEY, opt);
		}

		private string Decrypt(string data, AXMLSerializationSettings opt)
		{
			return AESThenHMAC.SimpleDecryptWithPassword(data, ENCRYPTION_KEY, opt);
		}

		public string GetDisplayIdentifier()
		{
			return Email;
		}
	}
}
