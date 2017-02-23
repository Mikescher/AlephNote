using AlephNote.PluginInterface;
using MSHC.Lang.Exceptions;
using MSHC.Util.Helper;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace AlephNote.Plugins.StandardNote
{
	public class StandardNoteData : IRemoteStorageSyncPersistance
	{
		public string SyncToken = "";

		public List<StandardFileTag> Tags = new List<StandardFileTag>(); 

		public XElement Serialize()
		{
			var data = new object[]
			{
				new XElement("SyncToken", SyncToken),
				new XElement("Tags", Tags.Select(t => 
					new XElement("Tag", 
						new XAttribute("UUID", t.UUID), 
						new XAttribute("Title", t.Title), 
						new XAttribute("EncryptionKey", t.EncryptionKey)))),
			};

			var r = new XElement("data", data);
			r.SetAttributeValue("plugin", StandardNotePlugin.Name);
			r.SetAttributeValue("pluginversion", StandardNotePlugin.Version.ToString());

			return r;
		}

		public void Deserialize(XElement input)
		{
			SyncToken = XHelper.GetChildValueString(input, "SyncToken");

			Tags = new List<StandardFileTag>();
			var child = input.Elements("Tags").FirstOrDefault();
			if (child == null) throw new XMLStructureException("Node not found: " + "Tags");
			foreach (var xtag in child.Elements("Tag")) 
				Tags.Add(new StandardFileTag
				{
					UUID = XHelper.GetAttributeGuid(xtag, "UUID"),
					Title = XHelper.GetAttributeString(xtag, "Title"),
					EncryptionKey = XHelper.GetAttributeString(xtag, "EncryptionKey"),
				});
		}

		public void UpdateTags(IEnumerable<StandardNoteAPI.SyncResultTag> retrievedTags, IEnumerable<StandardNoteAPI.SyncResultTag> savedTags, IEnumerable<StandardNoteAPI.SyncResultTag> unsavedTags)
		{
			foreach (var tag in retrievedTags.Concat(savedTags).Concat(unsavedTags))
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
						r.Title = tag.title;
						r.EncryptionKey = tag.enc_item_key;
					}
					else
					{
						Tags.Add(new StandardFileTag
						{
							UUID = tag.uuid,
							Title = tag.title,
							EncryptionKey = tag.item_key,
						});
					}
				}
			}
		}

		public List<StandardFileTag> GetUnusedTags(List<StandardNote> notes)
		{
			var tList = Tags.ToList();
			foreach (var strTag in notes.SelectMany(n => n.Tags))
			{
				tList.RemoveAll(t => t.Title == strTag);
			}
			return tList;
		}
	}
}
