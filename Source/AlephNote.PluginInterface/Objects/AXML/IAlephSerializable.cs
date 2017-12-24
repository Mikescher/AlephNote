namespace AlephNote.PluginInterface.Objects.AXML
{
	public interface IAlephSerializable
	{
		void OnBeforeSerialize();
		void OnAfterDeserialize();
	}
}
