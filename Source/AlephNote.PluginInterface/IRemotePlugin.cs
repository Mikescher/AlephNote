using AlephNote.PluginInterface.Util;
using System;
using System.Collections.Generic;
using System.Net;

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

		bool SupportsNativeDirectories { get; }
		bool SupportsPinning           { get; }
		bool SupportsLocking           { get; }
		bool SupportsTags              { get; }

		IRemoteStorageConfiguration CreateEmptyRemoteStorageConfiguration();
		IRemoteStorageConnection CreateRemoteStorageConnection(IWebProxy proxy, IRemoteStorageConfiguration config, HierachyEmulationConfig hierachicalConfig);
		IRemoteStorageSyncPersistance CreateEmptyRemoteSyncData();
		INote CreateEmptyNote(IRemoteStorageConnection conn, IRemoteStorageConfiguration cfg);

		IDictionary<string, string> GetHelpTexts();
	}
}
