using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Plugins.StandardNote
{
	[DebuggerDisplay("{Title} (( {UUID} ))")]
	public class StandardFileTagRef : IEquatable<StandardFileTagRef>
	{
		public readonly Guid? UUID;
		public readonly string Title;

		public StandardFileTagRef(Guid? id, string title)
		{
			UUID = id;
			Title = title;
		}

		public XElement Serialize()
		{
			return new XElement("tag", new XAttribute("ID", UUID?.ToString("P") ?? "null"), new XAttribute("Title", Title));
		}

		public static StandardFileTagRef Deserialize(XElement e)
		{
			var id   = XHelper.GetAttributeNGuid(e, "ID");
			var txt  = XHelper.GetAttributeString(e, "Title");

			return new StandardFileTagRef(id, txt);
		}
		
		public override int GetHashCode()
		{
			return (UUID==null ? 0 : UUID.GetHashCode())*113 + Title.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as StandardFileTagRef);
		}

		public bool Equals(StandardFileTagRef other)
		{
			if (other == null) return false;
			return UUID == other.UUID && Title == other.Title;
		}
		
		public static bool operator ==(StandardFileTagRef left, StandardFileTagRef right) => left?.Equals(right) ?? ReferenceEquals(right, null);

		public static bool operator !=(StandardFileTagRef left, StandardFileTagRef right) => !(left?.Equals(right) ?? ReferenceEquals(right, null));
	}
}
