using AlephNote.PluginInterface;
using System.Xml.Linq;

namespace AlephNote.Plugins.Headless
{
	public class HeadlessData : IRemoteStorageSyncPersistance
	{
		public XElement Serialize()
		{
			var r = new XElement("data");
			r.SetAttributeValue("plugin", HeadlessPlugin.Name);
			r.SetAttributeValue("pluginversion", HeadlessPlugin.Version.ToString());
			return r;
		}

		public void Deserialize(XElement input)
		{
			//
		}
	}
}
