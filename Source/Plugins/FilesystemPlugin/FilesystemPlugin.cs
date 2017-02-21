using AlephNote.PluginInterface;
using System;
using System.Net;

namespace AlephNote.Plugins.Filesystem
{
	public class FilesystemPlugin : BasicRemotePlugin
	{
		public static readonly Version Version = new Version(0, 0, 0, 1);
		public const string Name = "FilesystemPlugin";
		
		public FilesystemPlugin() : base("Filesystem (Human readable)", Version, Guid.Parse("a430b7ef-3526-4cbf-a304-8208de18efb5"))
		{
			//
		}

		public override IRemoteStorageConfiguration CreateEmptyRemoteStorageConfiguration()
		{
			return new FilesystemConfig();
		}

		public override IRemoteStorageConnection CreateRemoteStorageConnection(IWebProxy proxy, IRemoteStorageConfiguration config)
		{
			return new FilesystemConnection((FilesystemConfig)config);
		}

		public override INote CreateEmptyNote(IRemoteStorageConfiguration cfg)
		{
			return new FilesystemNote(Guid.NewGuid(), (FilesystemConfig)cfg);
		}

		public override IRemoteStorageSyncPersistance CreateEmptyRemoteSyncData()
		{
			return new FilesystemData();
		}
	}
}
