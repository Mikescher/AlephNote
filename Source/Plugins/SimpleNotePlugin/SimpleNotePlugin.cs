using CommonNote.PluginInterface;
using System;

namespace CommonNote.Plugins.SimpleNote
{
	public class SimpleNotePlugin : RemoteBasicProvider
	{
		public static readonly Version Version = new Version(0, 0, 0, 1);

		public SimpleNotePlugin() : base("Simplenote", Version, Guid.Parse("4c73e687-3803-4078-9bf0-554aaafc0873"))
		{
			//
		}

		public override IRemoteStorageConfiguration CreateEmptyRemoteStorageConfiguration()
		{
			return new SimpleNoteConfig();
		}

		public override IRemoteStorageConnection CreateRemoteStorageConnection(IRemoteStorageConfiguration config)
		{
			return new SimpleNoteConnection((SimpleNoteConfig)config);
		}

		public override INote CreateEmptyNode()
		{
			return new SimpleNote(Guid.NewGuid().ToString("N").ToUpper());
		}
	}
}
