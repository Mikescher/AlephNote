namespace AlephNote.Common.AlephXMLSerialization
{
	public interface IAlephSerializable
	{
		void OnBeforeSerialize();
		void OnAfterDeserialize();
	}
}
