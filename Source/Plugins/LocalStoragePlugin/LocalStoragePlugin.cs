using CommonNote.PluginInterface;
using System;

namespace CommonNote.Plugins.LocalStorage
{
	public class LocalStoragePlugin : ICommonNoteProvider
	{
		public static readonly Version Version = new Version(0, 0, 0, 1);

		public Guid GetUniqueID()
		{
			return Guid.Parse("37de6de1-26b0-41f5-b252-5e625d9ecfa3");
		}

		public string GetName()
		{
			return "Local Storage";
		}

		public Version GetVersion()
		{
			return Version;
		}

		public IRemoteStorageConfiguration CreateEmptyRemoteStorageConfiguration()
		{
			return new LocalStorageConfig();
		}

		public IRemoteStorageConnection CreateRemoteStorageConnection(IRemoteStorageConfiguration config)
		{
			return new LocalStorageConnection();
		}
	}
}
