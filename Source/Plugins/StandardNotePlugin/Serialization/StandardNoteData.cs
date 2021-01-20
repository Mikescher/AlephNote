using AlephNote.PluginInterface;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using AlephNote.PluginInterface.Util;
using MSHC.Serialization;
using System.Text;
using System;
using System.Globalization;

namespace AlephNote.Plugins.StandardNote
{
	public class StandardNoteData : IRemoteStorageSyncPersistance
	{
		public string SyncToken = "";
		public StandardNoteSessionData SessionData = null;

		public List<StandardFileTag> Tags = new List<StandardFileTag>(); 
		public List<StandardFileItemsKey> ItemsKeys = new List<StandardFileItemsKey>();

		public XElement Serialize()
		{
			var data = new object[]
			{
				TokenToXElem(),
				StandardNoteSessionData.Serialize("SessionData", SessionData),
				new XElement("Tags", Tags.Select(t => t.Serialize()).Cast<object>().ToArray()),
				new XElement("ItemsKeys", ItemsKeys.Select(t => t.Serialize()).Cast<object>().ToArray()),
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

			ItemsKeys = XHelper.GetChildOrNull(input, "ItemsKeys")?.Elements()?.Select(StandardFileItemsKey.Deserialize)?.ToList() ?? new List<StandardFileItemsKey>();
		}

		public XElement TokenToXElem()
		{
			var elem = XHelper2.TypeString.ToXElem("SyncToken", SyncToken);

			try
			{
				var content = Encoding.UTF8.GetString(Convert.FromBase64String(SyncToken));
				var split = content.Split(':');
				if (split.Length == 2)
				{
					elem.SetAttributeValue("version", split[0]);
					elem.SetAttributeValue("datetime", DateTimeOffset.FromUnixTimeSeconds((long)double.Parse(split[1], CultureInfo.InvariantCulture)).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
				}
			}
			catch (Exception)
			{
				// ignore
			}

			return elem;
		}

		public void UpdateTags(IEnumerable<StandardNoteAPI.SyncResultTag> retrievedTags, IEnumerable<StandardNoteAPI.SyncResultTag> savedTags, IEnumerable<(StandardNoteAPI.SyncResultTag unsavedtag, StandardNoteAPI.SyncResultTag servertag, string type)> conflictTags, IEnumerable<StandardNoteAPI.SyncResultTag> deletedTags)
		{
			foreach (var tag in retrievedTags.Concat(savedTags).Concat(conflictTags.Select(p => p.servertag).Where(p => p != null)).Concat(deletedTags))
			{
				if (tag.deleted)
				{
					var r = Tags.FirstOrDefault(p => p.UUID == tag.uuid);
					if (r != null) Tags.Remove(r);
				}
				else
				{
					var r = Tags.FirstOrDefault(p => p.UUID == tag.uuid);
					if (r != null) Tags.Remove(r);

					Tags.Add(new StandardFileTag(tag.uuid, tag.title, tag.created_at, tag.updated_at, tag.references.Where(rf => rf.content_type.ToLower() == "note").Select(rf => rf.uuid), tag.rawappdata));
				}
			}
		}

		public void UpdateKeys(IEnumerable<StandardNoteAPI.SyncResultItemsKey> retrievedKeys, IEnumerable<StandardNoteAPI.SyncResultItemsKey> savedKeys, IEnumerable<(StandardNoteAPI.SyncResultItemsKey unsavedkey, StandardNoteAPI.SyncResultItemsKey serverkey, string type)> conflictKeys, IEnumerable<StandardNoteAPI.SyncResultItemsKey> deletedKeys)
		{
			foreach (var itskey in retrievedKeys.Concat(savedKeys).Concat(conflictKeys.Select(p => p.serverkey).Where(p => p != null)).Concat(deletedKeys))
			{
				if (itskey.deleted)
				{
					var r = ItemsKeys.FirstOrDefault(p => p.UUID == itskey.uuid);
					if (r != null) ItemsKeys.Remove(r);
				}
				else
				{
					var r = ItemsKeys.FirstOrDefault(p => p.UUID == itskey.uuid);
					if (r != null) ItemsKeys.Remove(r);

					ItemsKeys.Add(new StandardFileItemsKey(itskey.uuid, itskey.version, itskey.created_at, itskey.updated_at, itskey.items_key, itskey.auth_key, itskey.isdefault, itskey.rawappdata));
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
