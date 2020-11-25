using AlephNote.PluginInterface.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;

namespace AlephNote.PluginInterface.Impl
{
	public abstract class BasicRemotePlugin : IRemotePlugin
	{
		private readonly Guid _uuid;
		private readonly string _name;
		private readonly Version _version;

		public string DisplayTitleLong { get { return GetName() + " v" + (_version.Revision == 0 ? _version.ToString(3) : (_version.ToString(4) + " BETA")); } }
		public string DisplayTitleShort { get { return GetName(); } }

		protected BasicRemotePlugin(string name, Version version, Guid uuid)
		{
			this._name = name;
			this._uuid = uuid;
			this._version = version;
		}

		public Guid GetUniqueID()
		{
			return _uuid;
		}

		public string GetName()
		{
			return _name;
		}

		public Version GetVersion()
		{
			return _version;
		}

		protected static Version GetInformationalVersion(Assembly assembly)
		{
			try
			{
				var vi = FileVersionInfo.GetVersionInfo(assembly.Location);
				return new Version(vi.FileMajorPart, vi.FileMinorPart, vi.FileBuildPart, vi.FilePrivatePart);
			}
			catch (Exception)
			{
				return new Version(0, 0, 0, 0);
			}
		}

		public IDictionary<string, string> GetHelpTexts()
		{
			return CreateHelpTexts().ToDictionary(t => t.Item1, t => t.Item2);
		}

		public abstract void Init(AlephLogger logger);

		public abstract IRemoteStorageConfiguration CreateEmptyRemoteStorageConfiguration();
		public abstract IRemoteStorageConnection CreateRemoteStorageConnection(IWebProxy proxy, IRemoteStorageConfiguration config, HierarchyEmulationConfig hierarchicalConfig);
		public abstract IRemoteStorageSyncPersistance CreateEmptyRemoteSyncData();
		public abstract INote CreateEmptyNote(IRemoteStorageConnection conn, IRemoteStorageConfiguration cfg);
		
		public abstract bool SupportsNativeDirectories         { get; }
		public abstract bool SupportsPinning                   { get; }
		public abstract bool SupportsLocking                   { get; }
		public abstract bool SupportsTags                      { get; }
		public abstract bool SupportsDownloadMultithreading    { get; }
		public abstract bool SupportsNewDownloadMultithreading { get; }
		public abstract bool SupportsUploadMultithreading      { get; }

		protected virtual IEnumerable<Tuple<string, string>> CreateHelpTexts()
		{
			return Enumerable.Empty<Tuple<string, string>>();
		}
	}
}
