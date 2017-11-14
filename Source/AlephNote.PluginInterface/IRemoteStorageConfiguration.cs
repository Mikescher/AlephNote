using System.Collections.Generic;
using System.Xml.Linq;

namespace AlephNote.PluginInterface
{
	public interface IRemoteStorageConfiguration
	{
		XElement Serialize();
		void Deserialize(XElement input);

		IEnumerable<DynamicSettingValue> ListProperties();
		void SetProperty(int id, string value);
		void SetProperty(int id, bool value);

		bool IsEqual(IRemoteStorageConfiguration other);
		IRemoteStorageConfiguration Clone();

		string GetDisplayIdentifier();
	}
}
