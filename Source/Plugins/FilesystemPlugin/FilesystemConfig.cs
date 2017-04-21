using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Plugins.Filesystem
{
	public class FilesystemConfig : IRemoteStorageConfiguration
	{
		private const int ID_FOLDER    = 6454;
		private const int ID_EXTENSION = 6455;
		private const int ID_ENCODING  = 6456;

		public string Folder    = string.Empty;
		public string Extension = "txt";
		public Encoding Encoding = Encoding.UTF8;

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
			yield return DynamicSettingValue.CreateFolderChooser(ID_FOLDER, "Folder", Folder);
			yield return DynamicSettingValue.CreateText(ID_EXTENSION, "Extension", Extension);
			yield return DynamicSettingValue.CreateCombobox(ID_ENCODING, "Encoding", Encoding.BodyName.ToUpper(), new[] { "UTF-8", "UTF-16", "UTF-32", "ASCII" });
		}

		public void SetProperty(int id, string value)
		{
			if (id == ID_FOLDER) Folder = value;
			if (id == ID_EXTENSION) Extension = value;
			if (id == ID_ENCODING) Encoding = Encoding.GetEncoding(value);
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
				return EncodingConverter.ByteToHexBitFiddleUppercase(md5.ComputeHash(Encoding.UTF8.GetBytes(Folder)));
			}
		}
	}
}
