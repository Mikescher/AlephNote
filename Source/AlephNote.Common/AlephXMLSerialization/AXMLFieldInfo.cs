using AlephNote.Common.AlephXMLSerialization;
using AlephNote.Common.Plugins;
using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace AlephNote.Settings
{
	public class AXMLFieldInfo
	{
		public enum SettingObjectTypeEnum
		{
			Integer,
			Double,
			NullableInteger,
			Boolean,
			Guid,
			EncryptedString,
			String,
			Enum,

			RemoteStorageAccount,
			List_RemoteStorageAccount,
		}

		public readonly SettingObjectTypeEnum ObjectType;
		public readonly PropertyInfo PropInfo;
		
		public AXMLFieldInfo(SettingObjectTypeEnum t, PropertyInfo i)
		{
			ObjectType = t;
			PropInfo = i;
		}

		public XElement Serialize(object objdata)
		{
			string resultdata;

			switch (ObjectType)
			{
				case SettingObjectTypeEnum.Integer:
					resultdata = Convert.ToString((int)objdata);
					break;

				case SettingObjectTypeEnum.NullableInteger:
					var nint = (int?)objdata;
					resultdata = nint == null ? string.Empty : nint.ToString();
					break;

				case SettingObjectTypeEnum.Boolean:
					resultdata = Convert.ToString((bool)objdata);
					break;

				case SettingObjectTypeEnum.Guid:
					resultdata = ((Guid)objdata).ToString("B");
					break;

				case SettingObjectTypeEnum.EncryptedString:
					resultdata = AlephXMLSerializerHelper.Encrypt((string)objdata);
					break;

				case SettingObjectTypeEnum.String:
					resultdata = (string)objdata;
					break;

				case SettingObjectTypeEnum.Double:
					resultdata = ((double)objdata).ToString("R");
					break;

				case SettingObjectTypeEnum.Enum:
					resultdata = Convert.ToString(objdata);
					break;

				case SettingObjectTypeEnum.RemoteStorageAccount:
					resultdata = ((RemoteStorageAccount)objdata).ID.ToString("B");
					break;

				case SettingObjectTypeEnum.List_RemoteStorageAccount:
					var x = new XElement(PropInfo.Name);
					x.Add(new XAttribute("type", SettingObjectTypeEnum.List_RemoteStorageAccount));
					x.Add(((IList<RemoteStorageAccount>)objdata).Select(SerializeRemoteStorageAccount));
					return x;

				default:
					throw new ArgumentOutOfRangeException("ObjectType", ObjectType, null);
			}

			return new XElement(PropInfo.Name, resultdata, new XAttribute("type", ObjectType));
		}

		public void Deserialize(object obj, XElement root)
		{
			var current = PropInfo.GetValue(obj);

			switch (ObjectType)
			{
				case SettingObjectTypeEnum.Integer:
					PropInfo.SetValue(obj, XHelper.GetChildValue(root, PropInfo.Name, (int)current));
					return;

				case SettingObjectTypeEnum.Double:
					PropInfo.SetValue(obj, XHelper.GetChildValue(root, PropInfo.Name, (double)current));
					return;

				case SettingObjectTypeEnum.NullableInteger:
					PropInfo.SetValue(obj, XHelper.GetChildValue(root, PropInfo.Name, (int?)current));
					return;

				case SettingObjectTypeEnum.Boolean:
					PropInfo.SetValue(obj, XHelper.GetChildValue(root, PropInfo.Name, (bool)current));
					return;

				case SettingObjectTypeEnum.Guid:
					PropInfo.SetValue(obj, XHelper.GetChildValue(root, PropInfo.Name, (Guid)current));
					return;

				case SettingObjectTypeEnum.EncryptedString:
					PropInfo.SetValue(obj, AlephXMLSerializerHelper.Decrypt(XHelper.GetChildValue(root, PropInfo.Name, AlephXMLSerializerHelper.Encrypt((string)current))));
					return;

				case SettingObjectTypeEnum.String:
					PropInfo.SetValue(obj, XHelper.GetChildValue(root, PropInfo.Name, (string)current));
					return;

				case SettingObjectTypeEnum.Enum:
					PropInfo.SetValue(obj, XHelper.GetChildValue(root, PropInfo.Name, current, PropInfo.PropertyType));
					break;

				case SettingObjectTypeEnum.RemoteStorageAccount:
					PropInfo.SetValue(obj, new RemoteStorageAccount(XHelper.GetChildValue(root, PropInfo.Name, Guid.Empty), null, null));
					break;

				case SettingObjectTypeEnum.List_RemoteStorageAccount:
					var list = (IList<RemoteStorageAccount>)current;
					list.Clear();
					foreach (var elem in XHelper.GetChildOrThrow(root, PropInfo.Name).Elements())
					{
						list.Add(DeserializeRemoteStorageAccount(elem));
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("ObjectType", ObjectType, null);
			}
		}

		private XElement SerializeRemoteStorageAccount(RemoteStorageAccount rsa)
		{
			var x = new XElement("Account");
			x.Add(new XAttribute("type", SettingObjectTypeEnum.RemoteStorageAccount));
			x.Add(new XElement("ID", rsa.ID.ToString("B"), new XAttribute("type", SettingObjectTypeEnum.Guid)));
			x.Add(new XElement("Plugin", rsa.Plugin.GetUniqueID().ToString("B"), new XAttribute("type", SettingObjectTypeEnum.Guid)));
			x.Add(new XElement("Config", rsa.Config.Serialize(), new XAttribute("type", "Generic")));
			return x;
		}

		private RemoteStorageAccount DeserializeRemoteStorageAccount(XElement e)
		{
			var rsa = new RemoteStorageAccount();
			rsa.ID = XHelper.GetChildValue(e, "ID", Guid.Empty);
			rsa.Plugin = PluginManagerSingleton.Inst.GetPlugin(XHelper.GetChildValue(e, "Plugin", Guid.Empty));
			rsa.Config = rsa.Plugin.CreateEmptyRemoteStorageConfiguration();
			rsa.Config.Deserialize(XHelper.GetChildOrThrow(e, "Config").Elements().Single());
			return rsa;
		}
	}


}
