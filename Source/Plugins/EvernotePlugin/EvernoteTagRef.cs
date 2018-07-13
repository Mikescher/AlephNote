using System;
using System.Xml.Linq;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Plugins.Evernote
{
	public class EvernoteTagRef : IEquatable<EvernoteTagRef>
	{
		public readonly Guid UUID;
		public readonly string Title;

		public EvernoteTagRef(Guid id, string title)
		{
			UUID = id;
			Title = title;
		}

		public XElement Serialize()
		{
			return new XElement("tag", new XAttribute("ID", UUID.ToString("P")), new XAttribute("Title", Title));
		}

		public static EvernoteTagRef Deserialize(XElement e)
		{
			var id = XHelper.GetAttributeGuid(e, "ID");
			var txt = XHelper.GetAttributeString(e, "Title");

			return new EvernoteTagRef(id, txt);
		}
		
		public override int GetHashCode()
		{
			return UUID.GetHashCode() * 113 + Title.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as EvernoteTagRef);
		}

		public bool Equals(EvernoteTagRef other)
		{
			if (other == null) return false;
			return UUID == other.UUID && Title == other.Title;
		}
		
		public static bool operator ==(EvernoteTagRef left, EvernoteTagRef right) => left?.Equals(right) ?? ReferenceEquals(right, null);

		public static bool operator !=(EvernoteTagRef left, EvernoteTagRef right) => !(left?.Equals(right) ?? ReferenceEquals(right, null));
	}
}
