using CommonNote.PluginInterface;
using System;

namespace CommonNote.Plugins.LocalStorage
{
	public class LocalStoragePlugin : RemoteBasicProvider
	{
		public static readonly Version Version = new Version(0, 0, 0, 1);

		public LocalStoragePlugin() : base("Local Storage", Version, Guid.Parse("37de6de1-26b0-41f5-b252-5e625d9ecfa3"))
		{
			//
		}

		public override IRemoteStorageConfiguration CreateEmptyRemoteStorageConfiguration()
		{
			return new LocalStorageConfig();
		}

		public override IRemoteStorageConnection CreateRemoteStorageConnection(IRemoteStorageConfiguration config)
		{
			return new LocalStorageConnection();
		}

		public override INote CreateEmptyNode()
		{
			return new LocalNote();
		}
	}
}
