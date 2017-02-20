using AlephNote.PluginInterface;
using MSHC.Math.Encryption;
using MSHC.Util.Helper;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace AlephNote.Plugins.Filesystem
{
	public class FilesystemConfig : IRemoteStorageConfiguration
	{
		private const int ID_FOLDER    = 6454;
		private const int ID_EXTENSION = 6454;

		public string Folder    = string.Empty;
		public string Extension = "txt";
		public Encoding Encoding = Encoding.UTF8;
		//TODO Encoding setting

		public XElement Serialize()
		{
			var data = new object[]
			{
				new XElement("Folder", Folder),
				new XElement("Extension", Extension),
			};

			var r = new XElement("config", data);
			r.SetAttributeValue("plugin", FilesystemPlugin.Name);
			r.SetAttributeValue("pluginversion", FilesystemPlugin.Version.ToString());
			return r;
		}

		public void Deserialize(XElement input)
		{
			if (input.Name.LocalName != "config") throw new Exception("LocalName != 'config'");

			Folder = XHelper.GetChildValue(input, "Folder", string.Empty);
			Extension = XHelper.GetChildValue(input, "Extension", "txt");
		}

		public IEnumerable<DynamicSettingValue> ListProperties()
		{
			yield return DynamicSettingValue.CreateText(ID_FOLDER, "Folder", Folder);
			yield return DynamicSettingValue.CreatePassword(ID_EXTENSION, "Extension", Extension);
		}

		public void SetProperty(int id, string value)
		{
			if (id == ID_FOLDER) Folder = value;
			if (id == ID_EXTENSION) Extension = value;
		}

		public void SetProperty(int id, bool value)
		{
			throw new ArgumentException();
		}

		public bool IsEqual(IRemoteStorageConfiguration iother)
		{
			var other = iother as FilesystemConfig;
			if (other == null) return false;

			if (this.Folder != other.Folder) return false;
			if (this.Extension != other.Extension) return false;

			return true;
		}

		public IRemoteStorageConfiguration Clone()
		{
			return new FilesystemConfig
			{
				Folder = this.Folder,
				Extension = this.Extension,
			};
		}

		public string GetUniqueName()
		{
			using (var md5 = MD5.Create())
			{
				return EncodingConverter.ByteToHexBitFiddle(md5.ComputeHash(Encoding.UTF8.GetBytes(Folder)));
			}
		}
	}
}
