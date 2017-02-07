
using System;

namespace CommonNote.PluginInterface
{
	public interface ICommonNoteProvider
	{
		string GetName();
		Version GetVersion();

		IRemoteStorageConfiguration CreateEmptyRemoteStorageConfiguration();
		IRemoteStorageConnection CreateRemoteStorageConnection(IRemoteStorageConfiguration config);
	}
}
