using AlephNote.PluginInterface;
using MSHC.Lang.Exceptions;
using MSHC.Serialization;
using MSHC.Util.Helper;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace AlephNote.Plugins.SimpleNote
{
	public class SimpleNoteData : IRemoteStorageSyncPersistance
	{
		public Dictionary<string, int> DeletedNotesCache = new Dictionary<string, int>();

		public XElement Serialize()
		{
			var data = new object[]
			{
				new XElement("DeletedNotes", DeletedNotesCache.Select(t => new XElement("Note", new XAttribute("version", t.Value), t.Key))),
			};

			var r = new XElement("data", data);
			r.SetAttributeValue("plugin", SimpleNotePlugin.Name);
			r.SetAttributeValue("pluginversion", SimpleNotePlugin.Version.ToString());

			return r;
		}

		public void Deserialize(XElement input)
		{
			DeletedNotesCache = new Dictionary<string, int>();

			var child = input.Elements("DeletedNotes").FirstOrDefault();
			if (child == null) throw new XMLStructureException("Node not found: " + "DeletedNotes");

			foreach (var noteTag in child.Elements("Note"))
			{
				DeletedNotesCache[noteTag.Value] = XHelper.GetAttributeInt(noteTag, "version");
			}
		}

		public void AddDeletedNote(string id, int vers)
		{
			if (DeletedNotesCache.ContainsKey(id))
			{
				if (DeletedNotesCache[id] < vers) DeletedNotesCache[id] = vers;
			}
			else
			{
				DeletedNotesCache[id] = vers;
			}
		}

		public void RemoveDeletedNote(string id)
		{
			DeletedNotesCache.Remove(id);
		}

		public bool IsDeleted(string id, int remoteVersion)
		{
			if (!DeletedNotesCache.ContainsKey(id)) return false;

			return DeletedNotesCache[id] >= remoteVersion;
		}
	}
}