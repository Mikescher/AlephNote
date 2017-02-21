using System;
using System.Net;

namespace AlephNote.PluginInterface
{
	public interface IRemotePlugin
	{
		string DisplayTitleLong { get; }
		string DisplayTitleShort { get; }

		Guid GetUniqueID();
		string GetName();
		Version GetVersion();

		IRemoteStorageConfiguration CreateEmptyRemoteStorageConfiguration();
		IRemoteStorageConnection CreateRemoteStorageConnection(IWebProxy proxy, IRemoteStorageConfiguration config);
		IRemoteStorageSyncPersistance CreateEmptyRemoteSyncData();
		INote CreateEmptyNote(IRemoteStorageConfiguration cfg);
	}

	public abstract class BasicRemotePlugin : IRemotePlugin
	{
		private readonly Guid uuid;
		private readonly string name;
		private readonly Version version;

		public string DisplayTitleLong { get { return GetName() + " v" + GetVersion(); } }
		public string DisplayTitleShort { get { return GetName(); } }

		protected BasicRemotePlugin(string name, Version version, Guid uuid)
		{
			this.name = name;
			this.uuid = uuid;
			this.version = version;
		}

		public Guid GetUniqueID()
		{
			return uuid;
		}

		public string GetName()
		{
			return name;
		}

		public Version GetVersion()
		{
			return version;
		}

		public abstract IRemoteStorageConfiguration CreateEmptyRemoteStorageConfiguration();
		public abstract IRemoteStorageConnection CreateRemoteStorageConnection(IWebProxy proxy, IRemoteStorageConfiguration config);
		public abstract IRemoteStorageSyncPersistance CreateEmptyRemoteSyncData();
		public abstract INote CreateEmptyNote(IRemoteStorageConfiguration cfg);
	}
}
