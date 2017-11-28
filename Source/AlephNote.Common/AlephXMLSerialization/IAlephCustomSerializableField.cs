using System.Xml.Linq;

namespace AlephNote.Common.AlephXMLSerialization
{
	public interface IAlephCustomSerializableField
	{
		object GetTypeStr();
		void Serialize(XElement target);
		object DeserializeNew(XElement source);
	}
}
