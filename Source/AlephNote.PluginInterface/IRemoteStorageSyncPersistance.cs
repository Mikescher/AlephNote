using System.Xml.Linq;

namespace AlephNote.PluginInterface
{
	public interface IRemoteStorageSyncPersistance
	{
		XElement Serialize();
		void Deserialize(XElement input);
	}
}
