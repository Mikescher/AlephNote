using AlephNote.PluginInterface;
using System.Xml.Linq;

namespace AlephNote.Plugins.Filesystem
{
	public class FilesystemData : IRemoteStorageSyncPersistance
	{
		public XElement Serialize()
		{
			var r = new XElement("data");
			r.SetAttributeValue("plugin", FilesystemPlugin.Name);
			r.SetAttributeValue("pluginversion", FilesystemPlugin.Version.ToString());
			return r;
		}

		public void Deserialize(XElement input)
		{
			//
		}
	}
}
