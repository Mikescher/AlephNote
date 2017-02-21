using AlephNote.PluginInterface;
using System.Xml.Linq;

namespace AlephNote.Plugins.Filesystem
{
	public class FilesystemData : IRemoteStorageSyncPersistance
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
