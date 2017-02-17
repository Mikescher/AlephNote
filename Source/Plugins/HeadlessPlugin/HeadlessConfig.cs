using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace AlephNote.Plugins.Headless
{
	class HeadlessConfig : IRemoteStorageConfiguration
	{
		public XElement Serialize()
		{
			var data = new object[]
			{
				//
			};

			var r = new XElement("config", data);
			r.SetAttributeValue("plugin", HeadlessPlugin.Name);
			r.SetAttributeValue("pluginversion", HeadlessPlugin.Version.ToString());
			return r;
		}

		public void Deserialize(XElement input)
		{
			//
		}

		public IEnumerable<DynamicSettingValue> ListProperties()
		{
			return Enumerable.Empty<DynamicSettingValue>();
		}

		public void SetProperty(int id, string value)
		{
			throw new ArgumentException();
		}

		public void SetProperty(int id, bool value)
		{
			throw new ArgumentException();
		}

		public bool IsEqual(IRemoteStorageConfiguration other)
		{
			return other is HeadlessConfig;
		}

		public IRemoteStorageConfiguration Clone()
		{
			return new HeadlessConfig();
		}

		public string GetUniqueName()
		{
			return "headless";
		}
	}
}
