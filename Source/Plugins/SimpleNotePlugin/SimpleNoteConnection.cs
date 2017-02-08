using CommonNote.PluginInterface;

namespace CommonNote.Plugins.SimpleNote
{
	class SimpleNoteConnection : IRemoteStorageConnection
	{
		private SimpleNoteConfig _config;

		public SimpleNoteConnection(SimpleNoteConfig config)
		{
			_config = config;
		}
	}
}
