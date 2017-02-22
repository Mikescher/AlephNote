using AlephNote.PluginInterface;
using System;
using System.Net;

namespace AlephNote.Plugins.StandardNote
{
	public class StandardNotePlugin : BasicRemotePlugin
	{
		public static readonly Version Version = new Version(0, 0, 0, 1);
		public const string Name = "StandardNotePlugin";

		private IAlephLogger _logger;

		public StandardNotePlugin() : base("Standard Notes", Version, Guid.Parse("30d867a4-cbdc-45c5-950a-c119bf2f2845"))
		{
			//
		}

		public override void Init(IAlephLogger logger)
		{
			_logger = logger;
		}

		public override IRemoteStorageConfiguration CreateEmptyRemoteStorageConfiguration()
		{
			return new StandardNoteConfig();
		}

		public override IRemoteStorageConnection CreateRemoteStorageConnection(IWebProxy proxy, IRemoteStorageConfiguration config)
		{
			return new StandardNoteConnection(_logger, proxy, (StandardNoteConfig)config);
		}

		public override INote CreateEmptyNote(IRemoteStorageConfiguration cfg)
		{
			return new StandardNote(Guid.NewGuid(), (StandardNoteConfig)cfg);
		}

		public override IRemoteStorageSyncPersistance CreateEmptyRemoteSyncData()
		{
			return new StandardNoteData();
		}
	}
}
