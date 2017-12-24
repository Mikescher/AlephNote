using System.Collections.Generic;
using System.Xml.Linq;
using AlephNote.PluginInterface.Objects;
using AlephNote.PluginInterface.Objects.AXML;

namespace AlephNote.PluginInterface
{
	public interface IRemoteStorageConfiguration
	{
		XElement Serialize(AXMLSerializationSettings opt);
		void Deserialize(XElement input, AXMLSerializationSettings opt);

		IEnumerable<DynamicSettingValue> ListProperties();
		void SetProperty(int id, string value);
		void SetProperty(int id, bool value);
		void SetProperty(int id, int value);

		bool IsEqual(IRemoteStorageConfiguration other);
		IRemoteStorageConfiguration Clone();

		string GetDisplayIdentifier();
	}
}
