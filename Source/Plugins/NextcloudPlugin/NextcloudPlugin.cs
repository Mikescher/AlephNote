using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Impl;
using AlephNote.PluginInterface.Util;
using System;
using System.Net;
using System.Reflection;

namespace AlephNote.Plugins.Nextcloud
{
	public class NextcloudPlugin : BasicRemotePlugin
	{
		public static readonly Version Version = GetInformationalVersion(typeof(NextcloudPlugin).GetTypeInfo().Assembly);
		public const string Name = "NextcloudPlugin";

		private IAlephLogger _logger;

		public NextcloudPlugin() : base("Nextcloud Notes", Version, Guid.Parse("9c4538de-8adb-438f-99fe-1531f90d9d0a"))
		{
			//
		}

		public override void Init(IAlephLogger logger)
		{
			_logger = logger;
		}

		public override IRemoteStorageConfiguration CreateEmptyRemoteStorageConfiguration()
		{
			return new NextcloudConfig();
		}

		public override IRemoteStorageConnection CreateRemoteStorageConnection(IWebProxy proxy, IRemoteStorageConfiguration config, HierachyEmulationConfig hConfig)
		{
			return new NextcloudConnection(_logger, proxy, (NextcloudConfig)config);
		}

		public override INote CreateEmptyNote(IRemoteStorageConnection iconn, IRemoteStorageConfiguration cfg)
		{
			return new NextcloudNote(-1, Guid.NewGuid(), (NextcloudConfig)cfg);
		}

		public override IRemoteStorageSyncPersistance CreateEmptyRemoteSyncData()
		{
			return new NextcloudData();
		}

		public override bool HasNativeDirectorySupport()
		{
			return true;
		}
	}
}
