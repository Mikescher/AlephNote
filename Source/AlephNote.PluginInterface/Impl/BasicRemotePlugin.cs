using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;

namespace AlephNote.PluginInterface.Impl
{
	public abstract class BasicRemotePlugin : IRemotePlugin
	{
		private readonly Guid uuid;
		private readonly string name;
		private readonly Version version;

		public string DisplayTitleLong { get { return GetName() + " v" + (version.Revision == 0 ? version.ToString(3) : (version.ToString(4) + " BETA")); } }
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

		protected static Version GetInformationalVersion(Assembly assembly)
		{
			try
			{
				var loc = assembly.Location;
				if (loc == null) return new Version(0, 0, 0, 0);
				var vi = FileVersionInfo.GetVersionInfo(loc);
				return new Version(vi.FileMajorPart, vi.FileMinorPart, vi.FileBuildPart, vi.FilePrivatePart);
			}
			catch (Exception)
			{
				return new Version(0, 0, 0, 0);
			}
		}

		public abstract void Init(IAlephLogger logger);

		public abstract IRemoteStorageConfiguration CreateEmptyRemoteStorageConfiguration();
		public abstract IRemoteStorageConnection CreateRemoteStorageConnection(IWebProxy proxy, IRemoteStorageConfiguration config);
		public abstract IRemoteStorageSyncPersistance CreateEmptyRemoteSyncData();
		public abstract INote CreateEmptyNote(IRemoteStorageConfiguration cfg);
	}
}
