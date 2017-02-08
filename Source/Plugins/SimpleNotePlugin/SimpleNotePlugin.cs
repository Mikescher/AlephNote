using CommonNote.PluginInterface;
using System;

namespace CommonNote.Plugins.SimpleNote
{
	public class SimpleNotePlugin : ICommonNoteProvider
	{
		public static readonly Version Version = new Version(0, 0, 0, 1);

		public Guid GetUniqueID()
		{
			return Guid.Parse("4c73e687-3803-4078-9bf0-554aaafc0873");
		}

		public string GetName()
		{
			return "Simplenote";
		}

		public Version GetVersion()
		{
			return Version;
		}

		public IRemoteStorageConfiguration CreateEmptyRemoteStorageConfiguration()
		{
			return new SimpleNoteConfig();
		}

		public IRemoteStorageConnection CreateRemoteStorageConnection(IRemoteStorageConfiguration config)
		{
			return new SimpleNoteConnection((SimpleNoteConfig)config);
		}
	}
}
