using AlephNote.PluginInterface;
using System.Xml.Linq;

namespace AlephNote.Plugins.Nextcloud
{
	public class NextcloudData : IRemoteStorageSyncPersistance
	{
		public XElement Serialize()
		{
			var r = new XElement("data");
			r.SetAttributeValue("plugin", NextcloudPlugin.Name);
			r.SetAttributeValue("pluginversion", NextcloudPlugin.Version.ToString());
			return r;
		}

		public void Deserialize(XElement input)
		{
			//
		}
	}
}
