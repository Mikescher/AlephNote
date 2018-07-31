using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Impl;
using AlephNote.PluginInterface.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace AlephNote.Plugins.Filesystem
{
	public class FilesystemPlugin : BasicRemotePlugin //TODO Option for FileWatcher
	{
		public static readonly Version Version = GetInformationalVersion(typeof(FilesystemPlugin).GetTypeInfo().Assembly);
		public const string Name = "FilesystemPlugin";

		public override bool SupportsPinning           => false;
		public override bool SupportsLocking           => true;
		public override bool SupportsNativeDirectories => true;
		public override bool SupportsTags              => false;

		public const int MIN_SEARCH_DEPTH =  1;
		public const int MAX_SEARCH_DEPTH = 16;

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

		public override IRemoteStorageConnection CreateRemoteStorageConnection(IWebProxy proxy, IRemoteStorageConfiguration config, HierachyEmulationConfig hConfig)
		{
			return new FilesystemConnection(logger, (FilesystemConfig)config);
		}

		public override INote CreateEmptyNote(IRemoteStorageConnection iconn, IRemoteStorageConfiguration cfg)
		{
			return new FilesystemNote(Guid.NewGuid(), (FilesystemConfig)cfg);
		}

		public override IRemoteStorageSyncPersistance CreateEmptyRemoteSyncData()
		{
			return new FilesystemData();
		}
		
		public override IEnumerable<Tuple<string, string>> CreateHelpTexts()
		{
			yield return Tuple.Create("SearchDepth", "The maximum folder depth for notes. Files in deeper nesting levels will be ignored.");
		}
	}
}
