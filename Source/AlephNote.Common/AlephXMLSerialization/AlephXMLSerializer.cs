using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using AlephNote.PluginInterface.Objects;
using AlephNote.PluginInterface.Objects.AXML;

namespace AlephNote.Common.AlephXMLSerialization
{
	public class AlephXMLSerializer<T> where T : IAlephSerializable
	{
		private class AttrObj { public PropertyInfo Info; public List<object> Attributes; }

		public const AXMLSerializationSettings DEFAULT_SERIALIZATION_SETTINGS = 
			AXMLSerializationSettings.FormattedOutput |
			AXMLSerializationSettings.SplittedBase64  |
			AXMLSerializationSettings.UseEncryption   |
			AXMLSerializationSettings.IncludeTypeInfo;

		private readonly string _rootNode;
		private readonly List<AXMLFieldInfo> _fields;

		// ReSharper disable once RedundantEnumerableCastCall
		public AlephXMLSerializer(string rootName)
		{
			_rootNode = rootName;

			_fields = typeof(T)
				.GetProperties()
				.Select(p => new AttrObj { Info = p, Attributes = p.GetCustomAttributes(typeof(AlephXMLFieldAttribute), false).Cast<object>().ToList() })
				.Where(p => p.Attributes.Count == 1)
				.Select(CreateFieldInfo)
				.ToList();
		}

		private AXMLFieldInfo CreateFieldInfo(AttrObj p)
		{
			var attr = (AlephXMLFieldAttribute)p.Attributes.Single();
			var type = GetSettingType(p.Info, attr.Encrypted);

			return new AXMLFieldInfo(type, p.Info);
		}

		private AXMLFieldInfo.SettingObjectTypeEnum GetSettingType(PropertyInfo prop, bool encrypt)
		{
			if (prop.PropertyType == typeof(int)) return AXMLFieldInfo.SettingObjectTypeEnum.Integer;
			if (prop.PropertyType == typeof(double)) return AXMLFieldInfo.SettingObjectTypeEnum.Double;
			if (prop.PropertyType == typeof(int?)) return AXMLFieldInfo.SettingObjectTypeEnum.NullableInteger;
			if (prop.PropertyType == typeof(string)) return encrypt ? AXMLFieldInfo.SettingObjectTypeEnum.EncryptedString : AXMLFieldInfo.SettingObjectTypeEnum.String;
			if (prop.PropertyType == typeof(bool)) return AXMLFieldInfo.SettingObjectTypeEnum.Boolean;
			if (prop.PropertyType == typeof(Guid)) return AXMLFieldInfo.SettingObjectTypeEnum.Guid;
			if (prop.PropertyType == typeof(Guid?)) return AXMLFieldInfo.SettingObjectTypeEnum.NGuid;
			if (prop.PropertyType.GetTypeInfo().IsEnum) return AXMLFieldInfo.SettingObjectTypeEnum.Enum;
			if (prop.PropertyType == typeof(RemoteStorageAccount)) return AXMLFieldInfo.SettingObjectTypeEnum.RemoteStorageAccount;
			if (prop.PropertyType == typeof(DirectoryPath)) return AXMLFieldInfo.SettingObjectTypeEnum.DirectoryPath;
			if (typeof(IList<RemoteStorageAccount>).IsAssignableFrom(prop.PropertyType)) return AXMLFieldInfo.SettingObjectTypeEnum.ListRemoteStorageAccount;
			if (typeof(IAlephCustomSerializableField).IsAssignableFrom(prop.PropertyType)) return AXMLFieldInfo.SettingObjectTypeEnum.CustomSerializable;

			throw new NotSupportedException("Setting of type " + prop.PropertyType + " not supported");
		}

		public string Serialize(T obj, AXMLSerializationSettings opt)
		{
			obj.OnBeforeSerialize();

			var root = new XElement(_rootNode);

			foreach (var prop in _fields)
			{
				var data = prop.PropInfo.GetValue(obj);

				root.Add(prop.Serialize(data, opt));
			}

			if ((opt & AXMLSerializationSettings.FormattedOutput) != 0)
				return XHelper.ConvertToStringFormatted(new XDocument(root));
			else
				return XHelper.ConvertToStringRaw(new XDocument(root));
		}

		public void Deserialize(T obj, string xml, AXMLSerializationSettings opt)
		{
			var xd = XDocument.Parse(xml);
			var root = xd.Root;
			if (root == null) throw new Exception("XDocument needs root");

			foreach (var prop in _fields)
			{
				prop.Deserialize(obj, root, opt);
			}

			obj.OnAfterDeserialize();
		}

		public void Clone(T source, T target)
		{
			var opt = AXMLSerializationSettings.None;

			var xml = Serialize(source, opt);
			Deserialize(target, xml, opt);
		}

		public bool IsEqual(T a, T b)
		{
			var opt = AXMLSerializationSettings.None;

			var xml1 = Serialize(a, opt);
			var xml2 = Serialize(b, opt);

			return xml1 == xml2;
		}
	}
}
