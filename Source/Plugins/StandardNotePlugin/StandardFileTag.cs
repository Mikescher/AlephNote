using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Plugins.StandardNote
{
	[DebuggerDisplay("{Title} (( {UUID} ))  + {References.Count} References")]
	public class StandardFileTag : IEquatable<StandardFileTag>
	{
		public readonly Guid? UUID;
		public readonly string Title;
		public readonly IReadOnlyList<Guid> References;

		public StandardFileTag(Guid? id, string title, IEnumerable<Guid> refs)
		{
			UUID = id;
			Title = title;
			References = refs.ToList();
		}

		public XElement Serialize()
		{
			var x = new XElement("tag", new XAttribute("ID", UUID?.ToString("P") ?? "null"), new XAttribute("Title", Title));
			x.Add(new XElement("References", References.Select(r => new XElement("ref", r.ToString("P")))));
			return x;
		}

		public static StandardFileTag Deserialize(XElement e)
		{
			var id   = XHelper.GetAttributeNGuid(e, "ID");
			var txt  = XHelper.GetAttributeString(e, "Title");
			var refs = XHelper.HasChild(e, "References") ? XHelper.GetChildValueStringList(e, "References", "ref").Select(Guid.Parse).ToList() : new List<Guid>();

			return new StandardFileTag(id, txt, refs);
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
