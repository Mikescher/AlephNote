using AlephNote.PluginInterface;
using System;
using System.Net;

namespace AlephNote.Plugins.Headless
{
	public class HeadlessPlugin : BasicRemotePlugin
	{
		public static readonly Version Version = new Version(0, 0, 0, 1);
		public const string Name = "HeadlessPlugin";

		public HeadlessPlugin() : base("No Remote", Version, Guid.Parse("37de6de1-26b0-41f5-b252-5e625d9ecfa3"))
		{
			//
		}

		public override void Init(IAlephLogger logger)
		{
			//
		}

		public override IRemoteStorageConfiguration CreateEmptyRemoteStorageConfiguration()
		{
			return new HeadlessConfig();
		}

		public override IRemoteStorageConnection CreateRemoteStorageConnection(IWebProxy proxy, IRemoteStorageConfiguration config)
		{
			return new HeadlessConnection();
		}

		public override INote CreateEmptyNote(IRemoteStorageConfiguration cfg)
		{
			return new HeadlessNote(Guid.NewGuid());
		}

		public override IRemoteStorageSyncPersistance CreateEmptyRemoteSyncData()
		{
			return new HeadlessData();
		}
	}
}
