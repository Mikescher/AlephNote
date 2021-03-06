﻿using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using AlephNote.PluginInterface.Objects;
using AlephNote.PluginInterface.Objects.AXML;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Plugins.Headless
{
	class HeadlessConfig : IRemoteStorageConfiguration
	{
		private const int ID_NAME = 6651;

		public string Name = "headless";

		public XElement Serialize(AXMLSerializationSettings opt)
		{
			var data = new object[]
			{
				new XElement("Name", Name),
			};

			var r = new XElement("config", data);
			r.SetAttributeValue("plugin", HeadlessPlugin.Name);
			r.SetAttributeValue("pluginversion", HeadlessPlugin.Version.ToString());
			return r;
		}

		public void Deserialize(XElement input, AXMLSerializationSettings opt)
		{
			if (input.Name.LocalName != "config") throw new Exception("LocalName != 'config'");

			Name = XHelper.GetChildValue(input, "Name", "headless");
		}

		public IEnumerable<DynamicSettingValue> ListProperties()
		{
			yield return DynamicSettingValue.CreateText(ID_NAME, "Name", Name);
		}

		public void SetProperty(int id, string value)
		{
			if (id == ID_NAME) Name = value;
		}

		public void SetProperty(int id, bool value)
		{
			throw new ArgumentException();
		}

		public void SetProperty(int id, int value)
		{
			throw new NotImplementedException();
		}

		public void SetEnumProperty(int id, object value, Type valueType)
		{
			throw new NotSupportedException();
		}

		public bool IsEqual(IRemoteStorageConfiguration iother)
		{
			var other = iother as HeadlessConfig;
			if (other == null) return false;

			if (this.Name != other.Name) return false;

			return true;
		}

		public IRemoteStorageConfiguration Clone()
		{
			return new HeadlessConfig();
		}

		public string GetDisplayIdentifier()
		{
			return Name;
		}
	}
}
