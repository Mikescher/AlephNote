using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Impl;
using System;
using System.Net;
using System.Reflection;

namespace AlephNote.Plugins.Filesystem
{
	public class FilesystemPlugin : BasicRemotePlugin
	{
		public static readonly Version Version = GetInformationalVersion(Assembly.GetExecutingAssembly());
		public const string Name = "FilesystemPlugin";

		private IAlephLogger logger;

		public FilesystemPlugin() : base("Filesystem", Version, Guid.Parse("a430b7ef-3526-4cbf-a304-8208de18efb5"))
		{
			//
		}

		public override void Init(IAlephLogger l)
		{
			logger = l;
		}

		public override IRemoteStorageConfiguration CreateEmptyRemoteStorageConfiguration()
		{
			return new FilesystemConfig();
		}

		public override IRemoteStorageConnection CreateRemoteStorageConnection(IWebProxy proxy, IRemoteStorageConfiguration config)
		{
			return new FilesystemConnection(logger, (FilesystemConfig)config);
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
