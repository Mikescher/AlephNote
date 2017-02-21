using AlephNote.PluginInterface;
using System.Xml.Linq;

namespace AlephNote.Plugins.Headless
{
	public class HeadlessData : IRemoteStorageSyncPersistance
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
