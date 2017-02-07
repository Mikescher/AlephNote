using CommonNote.PluginInterface;
using System;

namespace CommonNote.Plugins.StandardNote
{
	public class StandardNotePlugin : ICommonNoteProvider
	{
		public string GetName()
		{
			return "Standard Notes";
		}

		public Version GetVersion()
		{
			return new Version(0, 0, 0, 1);
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
