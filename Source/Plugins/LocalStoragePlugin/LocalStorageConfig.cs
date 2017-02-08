using CommonNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CommonNote.Plugins.LocalStorage
{
	class LocalStorageConfig : IRemoteStorageConfiguration
	{
		public XElement Serialize()
		{
			return new XElement("config");
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

		public bool IsEqual(IRemoteStorageConfiguration other)
		{
			return other is LocalStorageConfig;
		}

		public IRemoteStorageConfiguration Clone()
		{
			return new LocalStorageConfig();
		}
	}
}
