
using System;

namespace CommonNote.PluginInterface
{
	public interface ICommonNoteProvider
	{
		Guid GetUniqueID();
		string GetName();
		Version GetVersion();

		IRemoteStorageConfiguration CreateEmptyRemoteStorageConfiguration();
		IRemoteStorageConnection CreateRemoteStorageConnection(IRemoteStorageConfiguration config);
	}
}
