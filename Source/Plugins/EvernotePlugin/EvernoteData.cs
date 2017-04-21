using AlephNote.PluginInterface;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Plugins.Evernote
{
	public class EvernoteData : IRemoteStorageSyncPersistance
	{
		public int SyncStateUpdateCount = -1;

		public List<EvernoteTagRef> Tags = new List<EvernoteTagRef>(); 

		public XElement Serialize()
		{
			var data = new object[]
			{
				new XElement("SyncStateUpdateCount", SyncStateUpdateCount),
				new XElement("Tags", Tags.Select(t => t.Serialize()).Cast<object>().ToArray()),
			};

			var r = new XElement("data", data);
			r.SetAttributeValue("plugin", EvernotePlugin.Name);
			r.SetAttributeValue("pluginversion", EvernotePlugin.Version.ToString());

			return r;
		}

		public void Deserialize(XElement input)
		{
			SyncStateUpdateCount = XHelper.GetChildValueInt(input, "SyncStateUpdateCount");

			Tags = XHelper.GetChildOrThrow(input, "Tags").Elements().Select(EvernoteTagRef.Deserialize).ToList();
		}
	}
}
