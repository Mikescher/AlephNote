using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;

namespace AlephNote.PluginInterface
{
	public interface IRemotePlugin
	{
		string DisplayTitleLong { get; }
		string DisplayTitleShort { get; }

		void Init(IAlephLogger logger);

		Guid GetUniqueID();
		string GetName();
		Version GetVersion(); //SemVer. set last digit <> 0 to create a debug version (will not be loaded in RELEASE) 

		IRemoteStorageConfiguration CreateEmptyRemoteStorageConfiguration();
		IRemoteStorageConnection CreateRemoteStorageConnection(IWebProxy proxy, IRemoteStorageConfiguration config);
		IRemoteStorageSyncPersistance CreateEmptyRemoteSyncData();
		INote CreateEmptyNote(IRemoteStorageConfiguration cfg);
	}
}
