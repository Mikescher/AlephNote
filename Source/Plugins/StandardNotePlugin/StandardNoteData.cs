using AlephNote.PluginInterface;
using MSHC.Util.Helper;
using System.Xml.Linq;

namespace AlephNote.Plugins.StandardNote
{
	public class StandardNoteData : IRemoteStorageSyncPersistance
	{
		public string SyncToken = "";

		public XElement Serialize()
		{
			var data = new object[]
			{
				new XElement("SyncToken", SyncToken),
			};

			var r = new XElement("data", data);
			r.SetAttributeValue("plugin", StandardNotePlugin.Name);
			r.SetAttributeValue("pluginversion", StandardNotePlugin.Version.ToString());

			return r;
		}

		public void Deserialize(XElement input)
		{
			SyncToken = XHelper.GetChildValueString(input, "SyncToken");
		}
	}
}
