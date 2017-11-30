using AlephNote.Common.AlephXMLSerialization;
using System.Xml.Linq;

namespace AlephNote.Common.Settings.Types
{
	public class SnippetDefinition : IAlephCustomSerializableField
	{
		public static readonly SnippetDefinition DEFAULT = new SnippetDefinition(string.Empty, string.Empty);

		public readonly string DisplayName;
		public readonly string Value;

		public SnippetDefinition(string displ, string val)
		{
			DisplayName = displ;
			Value = val;
		}

		public object DeserializeNew(XElement source)
		{
			return new SnippetDefinition(source.Attribute("display").Value, source.Attribute("value").Value);
		}

		public object GetTypeStr()
		{
			return "SnippetDefinition";
		}

		public void Serialize(XElement target)
		{
			target.Add(new XAttribute("display", DisplayName));
			target.Add(new XAttribute("value", Value));
		}
	}
}
