using AlephNote.PluginInterface;
using System.Xml.Linq;

namespace AlephNote.Plugins.Nextcloud
{
	public class NextcloudData : IRemoteStorageSyncPersistance
	{
		public XElement Serialize()
		{
			return new XElement("data");
		}

		public void Deserialize(XElement input)
		{
			//
		}
	}
}
