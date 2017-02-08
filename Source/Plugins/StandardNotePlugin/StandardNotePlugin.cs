using CommonNote.PluginInterface;
using System;

namespace CommonNote.Plugins.StandardNote
{
	public class StandardNotePlugin : ICommonNoteProvider
	{
		public static readonly Version Version = new Version(0, 0, 0, 1);

		public Guid GetUniqueID()
		{
			return Guid.Parse("30d867a4-cbdc-45c5-950a-c119bf2f2845");
		}

		public string GetName()
		{
			return "Standard Notes";
		}

		public Version GetVersion()
		{
			return Version;
		}

		public IRemoteStorageConfiguration CreateEmptyRemoteStorageConfiguration()
		{
			throw new NotImplementedException();
		}

		public IRemoteStorageConnection CreateRemoteStorageConnection(IRemoteStorageConfiguration config)
		{
			throw new NotImplementedException();
		}
    }
}
