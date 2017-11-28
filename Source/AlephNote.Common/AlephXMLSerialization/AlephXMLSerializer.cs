using AlephNote.Common.Settings.Types;
using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Util;
using AlephNote.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace AlephNote.Common.AlephXMLSerialization
{
	public class AlephXMLSerializer<T> where T : IAlephSerializable
	{
		private class AttrObj { public PropertyInfo Info; public List<object> Attributes; }

		private readonly Type _rootType;
		private readonly string _rootNode;

		private List<AXMLFieldInfo> _fields;

		public AlephXMLSerializer(string rootName)
		{
			_rootType = typeof(T);
			_rootNode = rootName;

			_fields = _rootType
				.GetProperties()
				.Select(p => new AttrObj { Info = p, Attributes = p.GetCustomAttributes(typeof(AlephXMLFieldAttribute), false).Cast<object>().ToList() })
				.Where(p => p.Attributes.Count == 1)
				.Select(p => CreateFieldInfo(p))
				.ToList();
		}

		private AXMLFieldInfo CreateFieldInfo(AttrObj p)
		{
			var attr = (AlephXMLFieldAttribute)p.Attributes.Single();
			var type = GetSettingType(p.Info, attr.Encrypted);

			return new AXMLFieldInfo(type, p.Info);
		}

		public AXMLFieldInfo.SettingObjectTypeEnum GetSettingType(PropertyInfo prop, bool encrypt)
		{
			if (prop.PropertyType == typeof(int)) return AXMLFieldInfo.SettingObjectTypeEnum.Integer;
			if (prop.PropertyType == typeof(double)) return AXMLFieldInfo.SettingObjectTypeEnum.Double;
			if (prop.PropertyType == typeof(int?)) return AXMLFieldInfo.SettingObjectTypeEnum.NullableInteger;
			if (prop.PropertyType == typeof(string)) return encrypt ? AXMLFieldInfo.SettingObjectTypeEnum.EncryptedString : AXMLFieldInfo.SettingObjectTypeEnum.String;
			if (prop.PropertyType == typeof(bool)) return AXMLFieldInfo.SettingObjectTypeEnum.Boolean;
			if (prop.PropertyType == typeof(Guid)) return AXMLFieldInfo.SettingObjectTypeEnum.Guid;
			if (prop.PropertyType.GetTypeInfo().IsEnum) return AXMLFieldInfo.SettingObjectTypeEnum.Enum;
			if (prop.PropertyType == typeof(RemoteStorageAccount)) return AXMLFieldInfo.SettingObjectTypeEnum.RemoteStorageAccount;
			if (typeof(IList<RemoteStorageAccount>).IsAssignableFrom(prop.PropertyType)) return AXMLFieldInfo.SettingObjectTypeEnum.List_RemoteStorageAccount;
			if (typeof(IAlephCustomSerializableField).IsAssignableFrom(prop.PropertyType)) return AXMLFieldInfo.SettingObjectTypeEnum.CustomSerializable;

			throw new NotSupportedException("Setting of type " + prop.PropertyType + " not supported");
		}

		public string Serialize(T obj)
		{
			var root = new XElement(_rootNode);

			foreach (var prop in _fields)
			{
				var data = prop.PropInfo.GetValue(obj);

				root.Add(prop.Serialize(data));
			}

			return XHelper.ConvertToString(new XDocument(root));
		}

		public void Deserialize(T obj, string xml)
		{
			var xd = XDocument.Parse(xml);
			var root = xd.Root;
			if (root == null) throw new Exception("XDocument needs root");

			foreach (var prop in _fields)
			{
				prop.Deserialize(obj, root);
			}

			obj.OnAfterDeserialize();
		}

		public void Clone(T source, T target)
		{
			var xml = Serialize(source);
			Deserialize(target, xml);
		}

		public bool IsEqual(T a, T b)
		{
			var xml1 = Serialize(a);
			var xml2 = Serialize(b);

			return xml1 == xml2;
		}
	}
}
