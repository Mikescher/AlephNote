using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using AlephNote.PluginInterface.Objects.AXML;
using AlephNote.PluginInterface.Util;
using MSHC.Lang.Attributes;

namespace AlephNote.Plugins.StandardNote
{
	public enum MDateSource
	{
		[EnumDescriptor("From Server")]
		RawServer,

		[EnumDescriptor("From Metadata")]
		Metadata,

		[EnumDescriptor("Intelligent")]
		Intelligent,

		[EnumDescriptor("Intelligent (content changes only)")]
		IntelligentContent,
	}

	public class StandardNoteConfig : IRemoteStorageConfiguration
	{
		private const string ENCRYPTION_KEY = @"HuIpJachKuRyJuOmVelThufCeck"; // https://duckduckgo.com/?q=random+password+32+characters

		public static readonly Regex REX_LINEBREAK = new Regex(@"\r?\n", RegexOptions.Compiled);

		private const int ID_EMAIL         = 6251;
		private const int ID_PASSWORD      = 6252;
		private const int ID_SYNCSERVER    = 6253;
		private const int ID_APISERVER     = 6258;
		private const int ID_REMTAGS       = 6255;
		private const int ID_HIERARCHYTAGS = 6256;
		private const int ID_MDATESOURCE   = 6257;

		public string      Email                  = string.Empty;
		public string      Password               = string.Empty;
		public string      SyncServer             = @"https://sync.standardnotes.org";
		public string      APIServer              = @"https://api.standardnotes.com";
		public bool        RemEmptyTags           = true;
		public bool        CreateHierarchyTags    = false;
		public MDateSource ModificationDateSource = MDateSource.Metadata;

		public XElement Serialize(AXMLSerializationSettings opt)
		{
			var data = new object[]
			{
				new XElement("Email", Email),
				new XElement("Password", Encrypt(Password, opt)),
				new XElement("Server", SyncServer),
				new XElement("APIServer", APIServer),
				new XElement("RemEmptyTags", RemEmptyTags),
				new XElement("CreateHierarchyTags", CreateHierarchyTags),
				new XElement("ModificationDateSource", ModificationDateSource),
			};

			var r = new XElement("config", data);
			r.SetAttributeValue("plugin", StandardNotePlugin.Name);
			r.SetAttributeValue("pluginversion", StandardNotePlugin.Version.ToString());
			return r;
		}

		public void Deserialize(XElement input, AXMLSerializationSettings opt)
		{
			if (input.Name.LocalName != "config") throw new Exception("LocalName != 'config'");

			Email                  = XHelper.GetChildValue(input, "Email", Email);
			Password               = Decrypt(XHelper.GetChildValue(input, "Password", string.Empty), opt);
			SyncServer             = XHelper.GetChildValue(input, "Server", SyncServer);
			APIServer              = XHelper.GetChildValue(input, "APIServer", APIServer);
			RemEmptyTags           = XHelper.GetChildValue(input, "RemEmptyTags", RemEmptyTags);
			CreateHierarchyTags    = XHelper.GetChildValue(input, "CreateHierarchyTags", CreateHierarchyTags);
			ModificationDateSource = XHelper.GetChildValue(input, "ModificationDateSource", ModificationDateSource);
		}

		public IEnumerable<DynamicSettingValue> ListProperties()
		{
			yield return DynamicSettingValue.CreateText(ID_EMAIL, "Email", Email);
			yield return DynamicSettingValue.CreatePassword(ID_PASSWORD, "Password", Password);
			yield return DynamicSettingValue.CreateText(ID_SYNCSERVER, "Sync Server", SyncServer);
			yield return DynamicSettingValue.CreateText(ID_APISERVER, "API Server", APIServer);
			yield return DynamicSettingValue.CreateCheckbox(ID_REMTAGS, "Delete unused tags", RemEmptyTags, "RemEmptyTags");
			yield return DynamicSettingValue.CreateEnumCombobox(ID_MDATESOURCE, "Source for modification date", ModificationDateSource, "ModificationDateSource");
			yield return DynamicSettingValue.CreateCheckbox(ID_HIERARCHYTAGS, "Create folder tags", CreateHierarchyTags, "CreateHierarchyTags");
			yield return DynamicSettingValue.CreateHyperlink("Create Standard Notes account", "https://standardnotes.org/");
		}

		public void SetProperty(int id, string value)
		{
			if (id == ID_EMAIL)       Email                  = value;
			if (id == ID_PASSWORD)    Password               = value;
			if (id == ID_SYNCSERVER)  SyncServer             = value;
			if (id == ID_APISERVER)   APIServer              = value;
		}

		public void SetProperty(int id, bool value)
		{
			if (id == ID_REMTAGS)       RemEmptyTags        = value;
			if (id == ID_HIERARCHYTAGS) CreateHierarchyTags = value;
		}

		public void SetProperty(int id, int value)
		{
			throw new NotSupportedException();
		}

		public void SetEnumProperty(int id, object value, Type valueType)
		{
			if (id == ID_MDATESOURCE) ModificationDateSource = (MDateSource)value;
		}

		public bool IsEqual(IRemoteStorageConfiguration iother)
		{
			var other = iother as StandardNoteConfig;
			if (other == null) return false;

			if (this.Email                  != other.Email)                  return false;
			if (this.Password               != other.Password)               return false;
			if (this.SyncServer             != other.SyncServer)             return false;
			if (this.APIServer              != other.APIServer)              return false;
			if (this.RemEmptyTags           != other.RemEmptyTags)           return false;
			if (this.CreateHierarchyTags    != other.CreateHierarchyTags)    return false;
			if (this.ModificationDateSource != other.ModificationDateSource) return false;

			return true;
		}

		public IRemoteStorageConfiguration Clone()
		{
			return new StandardNoteConfig
			{
				Email                  = this.Email,
				Password               = this.Password,
				SyncServer             = this.SyncServer,
				APIServer              = this.APIServer,
				RemEmptyTags           = this.RemEmptyTags,
				CreateHierarchyTags    = this.CreateHierarchyTags,
				ModificationDateSource = this.ModificationDateSource,
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
			return Email;
		}
	}
}
