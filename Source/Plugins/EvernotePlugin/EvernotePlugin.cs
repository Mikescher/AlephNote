using System;
using System.Net;
using System.Reflection;
using AlephNote.PluginInterface;

namespace AlephNote.Plugins.Evernote
{
	public class EvernotePlugin : BasicRemotePlugin
	{
		public static readonly Version Version = GetInformationalVersion(Assembly.GetExecutingAssembly());
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

		public override IRemoteStorageConnection CreateRemoteStorageConnection(IWebProxy proxy, IRemoteStorageConfiguration config)
		{
			return new EvernoteConnection(_logger, proxy, (EvernoteConfig)config);
		}

		public override INote CreateEmptyNote(IRemoteStorageConfiguration cfg)
		{
			return new EvernoteNote(Guid.NewGuid(), (EvernoteConfig)cfg);
		}

		public override IRemoteStorageSyncPersistance CreateEmptyRemoteSyncData()
		{
			return new EvernoteData();
		}
	}
}
