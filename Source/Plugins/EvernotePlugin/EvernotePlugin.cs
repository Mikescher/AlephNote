using System;
using System.Net;
using System.Reflection;
using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Impl;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Plugins.Evernote
{
	public class EvernotePlugin : BasicRemotePlugin
	{
		public static readonly Version Version = GetInformationalVersion(typeof(EvernotePlugin).GetTypeInfo().Assembly);
		public const string Name = "EvernotePlugin";

		private IAlephLogger _logger;

		public EvernotePlugin() : base("Evernote", Version, Guid.Parse("b1bc98fa-247a-4f12-ac2d-febaa927f2ec"))
		{
			//
		}

		public override void Init(IAlephLogger logger)
		{
			_logger = logger;
		}

		public override IRemoteStorageConfiguration CreateEmptyRemoteStorageConfiguration()
		{
			return new EvernoteConfig();
		}

		public override IRemoteStorageConnection CreateRemoteStorageConnection(IWebProxy proxy, IRemoteStorageConfiguration config, HierachyEmulationConfig hConfig)
		{
			return new EvernoteConnection(_logger, proxy, (EvernoteConfig)config, hConfig);
		}

		public override INote CreateEmptyNote(IRemoteStorageConnection iconn, IRemoteStorageConfiguration cfg)
		{
			var conn = (EvernoteConnection)iconn;
			return new EvernoteNote(Guid.NewGuid(), (EvernoteConfig)cfg, conn.HConfig);
		}

		public override IRemoteStorageSyncPersistance CreateEmptyRemoteSyncData()
		{
			return new EvernoteData();
		}

		public override bool HasNativeDirectorySupport()
		{
			return false;
		}
	}
}
