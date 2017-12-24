namespace AlephNote.PluginInterface.Objects.AXML
{
	public interface IAlephCustomFlatSerializableField
	{
		object GetTypeStr();
		string Serialize(AXMLSerializationSettings opt);
		object DeserializeNew(string source, AXMLSerializationSettings opt);
	}
}
