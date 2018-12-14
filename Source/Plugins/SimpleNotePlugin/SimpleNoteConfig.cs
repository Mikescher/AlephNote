using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using AlephNote.PluginInterface.Objects;
using AlephNote.PluginInterface.Objects.AXML;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Plugins.SimpleNote
{
	public class SimpleNoteConfig : IRemoteStorageConfiguration
	{
		private const string ENCRYPTION_KEY = @"rLPDWseNePtqLjXuYRdAAWjQnJoSjxjp"; // https://duckduckgo.com/?q=random+password+32+characters
		
		private const int ID_USERNAME = 6151;
		private const int ID_PASSWORD = 6152;
		private const int ID_PERMADEL = 6153;
		private const int ID_BLANKFMT = 6154;

		public string Username = string.Empty;
		public string Password = string.Empty;
		public bool PermanentlyDeleteNotes = false;
		public bool BlankLineBelowTitle = true;

		public XElement Serialize(AXMLSerializationSettings opt)
		{
			var data = new object[]
			{
				new XElement("Username", Username),
				new XElement("Password", Encrypt(Password, opt)),
				new XElement("PermanentlyDeleteNotes", PermanentlyDeleteNotes),
				new XElement("BlankLineBelowTitle", BlankLineBelowTitle),
			};

			var r = new XElement("config", data);
			r.SetAttributeValue("plugin", SimpleNotePlugin.Name);
			r.SetAttributeValue("pluginversion", SimpleNotePlugin.Version.ToString());
			return r;
		}

		public void Deserialize(XElement input, AXMLSerializationSettings opt)
		{
			if (input.Name.LocalName != "config") throw new Exception("LocalName != 'config'");

			Username = XHelper.GetChildValue(input, "Username", Username);
			Password = Decrypt(XHelper.GetChildValue(input, "Password", string.Empty), opt);
			PermanentlyDeleteNotes = XHelper.GetChildValue(input, "PermanentlyDeleteNotes", PermanentlyDeleteNotes);
			BlankLineBelowTitle = XHelper.GetChildValue(input, "BlankLineBelowTitle", BlankLineBelowTitle);
		}

		public IEnumerable<DynamicSettingValue> ListProperties()
		{
			yield return DynamicSettingValue.CreateText(ID_USERNAME, "Username", Username);
			yield return DynamicSettingValue.CreatePassword(ID_PASSWORD, "Password", Password);
			yield return DynamicSettingValue.CreateCheckbox(ID_PERMADEL, "Delete notes permanently on server", PermanentlyDeleteNotes, "PermanentlyDeleteNotes");
			yield return DynamicSettingValue.CreateCheckbox(ID_BLANKFMT, "Empty line between title and content", BlankLineBelowTitle, "BlankLineBelowTitle");
			yield return DynamicSettingValue.CreateHyperlink("Create Simplenote account", "https://simplenote.com/");
		}

		public void SetProperty(int id, string value)
		{
			if (id == ID_USERNAME) Username = value;
			if (id == ID_PASSWORD) Password = value;
		}

		public void SetProperty(int id, bool value)
		{
			if (id == ID_PERMADEL) PermanentlyDeleteNotes = value;
			if (id == ID_BLANKFMT) BlankLineBelowTitle = value;
		}

		public void SetProperty(int id, int value)
		{
			throw new NotImplementedException();
		}

		public bool IsEqual(IRemoteStorageConfiguration iother)
		{
			var other = iother as SimpleNoteConfig;
			if (other == null) return false;

			if (this.Username               != other.Username)               return false;
			if (this.Password               != other.Password)               return false;
			if (this.PermanentlyDeleteNotes != other.PermanentlyDeleteNotes) return false;
			if (this.BlankLineBelowTitle    != other.BlankLineBelowTitle)    return false;

			return true;
		}

		public IRemoteStorageConfiguration Clone()
		{
			return new SimpleNoteConfig
			{
				Username               = this.Username,
				Password               = this.Password,
				PermanentlyDeleteNotes = this.PermanentlyDeleteNotes,
				BlankLineBelowTitle    = this.BlankLineBelowTitle,
			};
		}

		private string Encrypt(string data, AXMLSerializationSettings opt)
		{
			return ANEncryptionHelper.SimpleEncryptWithPassword(data, ENCRYPTION_KEY, opt);
		}

		private string Decrypt(string data, AXMLSerializationSettings opt)
		{
			return ANEncryptionHelper.SimpleDecryptWithPassword(data, ENCRYPTION_KEY, opt);
		}

		public string GetDisplayIdentifier()
		{
			return Username;
		}
	}
}
