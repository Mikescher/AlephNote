using AlephNote.PluginInterface;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Plugins.StandardNote
{
	public class StandardNoteData : IRemoteStorageSyncPersistance
	{
		public string SyncToken = "";
		public StandardNoteSessionData SessionData = null;

		public List<StandardFileTag> Tags = new List<StandardFileTag>(); 

		public XElement Serialize()
		{
			var data = new object[]
			{
				new XElement("SyncToken", SyncToken),
				StandardNoteSessionData.Serialize("SessionData", SessionData),
				new XElement("Tags", Tags.Select(t => t.Serialize()).Cast<object>().ToArray()),
			};

			var r = new XElement("data", data);
			r.SetAttributeValue("plugin", StandardNotePlugin.Name);
			r.SetAttributeValue("pluginversion", StandardNotePlugin.Version.ToString());

			return r;
		}

		public void Deserialize(XElement input)
		{
			SyncToken = XHelper.GetChildValueString(input, "SyncToken");

			SessionData = StandardNoteSessionData.Deserialize(XHelper.GetChildOrNull(input, "SessionData"));

			Tags = XHelper.GetChildOrThrow(input, "Tags").Elements().Select(StandardFileTag.Deserialize).ToList();
		}

		public void UpdateTags(IEnumerable<StandardNoteAPI.SyncResultTag> retrievedTags, IEnumerable<StandardNoteAPI.SyncResultTag> savedTags, IEnumerable<StandardNoteAPI.SyncResultTag> unsavedTags, IEnumerable<StandardNoteAPI.SyncResultTag> deletedTags)
		{
			foreach (var tag in retrievedTags.Concat(savedTags).Concat(unsavedTags).Concat(deletedTags))
			{
				if (tag.deleted)
				{
					var r = Tags.FirstOrDefault(p => p.UUID == tag.uuid);
					if (r != null) Tags.Remove(r);
				}
				else
				{
					var r = Tags.FirstOrDefault(p => p.UUID == tag.uuid);
					if (r != null)
					{
						Tags.Remove(r);
						Tags.Add(new StandardFileTag(tag.uuid, tag.title, tag.references.Where(rf => rf.content_type == "Note").Select(rf => rf.uuid)));
					}
					else
					{
						Tags.Add(new StandardFileTag(tag.uuid, tag.title, tag.references.Where(rf => rf.content_type == "Note").Select(rf => rf.uuid)));
					}
				}
			}
		}

		public List<StandardFileTag> GetUnusedTags(List<StandardFileNote> notes)
		{
			var tList = Tags.ToList();
			foreach (var inTag in notes.SelectMany(n => n.InternalTags))
			{
				if (inTag.UUID != null) tList.RemoveAll(t => t.UUID == inTag.UUID);
			}
			return tList;
		}
	}
}
