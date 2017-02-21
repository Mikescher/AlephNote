using AlephNote.PluginInterface;
using System.Xml.Linq;

namespace AlephNote.Plugins.SimpleNote
{
	public class SimpleNoteData : IRemoteStorageSyncPersistance
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