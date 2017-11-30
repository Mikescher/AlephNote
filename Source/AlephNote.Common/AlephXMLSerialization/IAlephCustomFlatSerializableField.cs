using System.Xml.Linq;

namespace AlephNote.Common.AlephXMLSerialization
{
	public interface IAlephCustomFlatSerializableField
	{
		object GetTypeStr();
		string Serialize();
		object DeserializeNew(string source);
	}
}
