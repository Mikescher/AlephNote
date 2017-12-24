using System.Xml.Linq;

namespace AlephNote.PluginInterface.Objects.AXML
{
	public interface IAlephCustomSerializableField
	{
		object GetTypeStr();
		void Serialize(XElement target, AXMLSerializationSettings opt);
		object DeserializeNew(XElement source, AXMLSerializationSettings opt);
	}
}
