using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using AlephNote.PluginInterface.Util;
using MSHC.Lang.Collections;

namespace AlephNote.Plugins.StandardNote
{
	[DebuggerDisplay("{Title} (( {UUID} ))  + {References.Count} References")]
	public class StandardFileTag : IEquatable<StandardFileTag>
	{
		public readonly DateTimeOffset CreationDate;     // raw creation date from SN API
		public readonly DateTimeOffset ModificationDate; // raw modification date from SN API
		public readonly Guid? UUID;
		public readonly string Title;
		public readonly IReadOnlyList<Guid> References;

		public StandardFileTag(Guid? id, string title, DateTimeOffset cdate, DateTimeOffset mdate, IEnumerable<Guid> refs)
		{
			UUID = id;
			Title = title;
			CreationDate = cdate;
			ModificationDate = mdate;
			References = refs.ToList();
		}

		public XElement Serialize()
		{
			var x = new XElement("tag", 
				new XAttribute("ID", UUID?.ToString("P") ?? "null"),
				new XAttribute("Title", Title),
				new XAttribute("CreationDate", CreationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture)),
				new XAttribute("ModificationDate", ModificationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture)));

			x.Add(new XElement("References", References.Select(r => new XElement("ref", r.ToString("P")))));

			return x;
		}

		public static StandardFileTag Deserialize(XElement e)
		{
			var id    = XHelper.GetAttributeNGuid(e, "ID");
			var txt   = XHelper.GetAttributeString(e, "Title");
			var cdate = XHelper.GetAttributeDateTimeOffsetOrDefault(e, "CreationDate", DateTimeOffset.MinValue);
			var mdate = XHelper.GetAttributeDateTimeOffsetOrDefault(e, "ModificationDate", DateTimeOffset.Now);
			var refs  = XHelper.HasChild(e, "References") ? XHelper.GetChildValueStringList(e, "References", "ref").Select(Guid.Parse).ToList() : new List<Guid>();

			return new StandardFileTag(id, txt, cdate, mdate, refs);
		}
		
		public override int GetHashCode()
		{
			return (UUID==null ? 0 : UUID.GetHashCode())*113 + Title.GetHashCode() + 1523 * References.Sum(r => r.GetHashCode());
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as StandardFileTag);
		}

		public bool Equals(StandardFileTag other)
		{
			if (other == null) return false;
			return UUID == other.UUID && Title == other.Title && References.ListEquals(other.References, (a,b)=>a==b);
		}
		
		public static bool operator ==(StandardFileTag left, StandardFileTag right) => left?.Equals(right) ?? ReferenceEquals(right, null);

		public static bool operator !=(StandardFileTag left, StandardFileTag right) => !(left?.Equals(right) ?? ReferenceEquals(right, null));

		public StandardFileTagRef ToRef() => new StandardFileTagRef(UUID, Title);

		public bool ContainsReference(StandardFileNote note) => References.Any(r => r == note.ID);
	}
}
