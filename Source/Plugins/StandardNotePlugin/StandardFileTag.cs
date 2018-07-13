using System;
using System.Xml.Linq;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Plugins.StandardNote
{
	public class StandardFileTag : IEquatable<StandardFileTag>
	{
		public readonly Guid? UUID;
		public readonly string Title;

		public StandardFileTag(Guid? id, string title)
		{
			UUID = id;
			Title = title;
		}

		public XElement Serialize()
		{
			return new XElement("tag", new XAttribute("ID", UUID?.ToString("P") ?? "null"), new XAttribute("Title", Title));
		}

		public static StandardFileTag Deserialize(XElement e)
		{
			var id = XHelper.GetAttributeNGuid(e, "ID");
			var txt = XHelper.GetAttributeString(e, "Title");

			return new StandardFileTag(id, txt);
		}
		
		public override int GetHashCode()
		{
			return (UUID==null ? 0 : UUID.GetHashCode())*113 + Title.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as StandardFileTag);
		}

		public bool Equals(StandardFileTag other)
		{
			if (other == null) return false;
			return UUID == other.UUID && Title == other.Title;
		}
		
		public static bool operator ==(StandardFileTag left, StandardFileTag right) => left?.Equals(right) ?? ReferenceEquals(right, null);

		public static bool operator !=(StandardFileTag left, StandardFileTag right) => !(left?.Equals(right) ?? ReferenceEquals(right, null));
	}
}
