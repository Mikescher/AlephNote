using System;

namespace AlephNote.PluginInterface
{
	public class RemoteStorageAccount
	{
		public Guid ID { get { return _id; } set { _id = value; } }
		private Guid _id;

		public IRemotePlugin Plugin { get { return _plugin; } set { _plugin = value; } }
		private IRemotePlugin _plugin;

		public IRemoteStorageConfiguration Config { get { return _config; } set { _config = value; } }
		private IRemoteStorageConfiguration _config;

		public string DisplayTitle
		{
			get
			{
				var di = _config.GetDisplayIdentifier();
				var ds = _plugin.DisplayTitleShort;
				return (string.IsNullOrWhiteSpace(di)) ? ($"{ds} (new..)") : ($"{ds} ({di})");
			}	
		}

		public RemoteStorageAccount()
		{
			// for deserialization
		}

		public RemoteStorageAccount(Guid id, IRemotePlugin plugin, IRemoteStorageConfiguration cfg)
		{
			_plugin = plugin;
			_id = id;
			_config = cfg;
		}

		public bool IsEqual(RemoteStorageAccount other)
		{
			return
				_id == other._id &&
				_plugin.GetUniqueID() == other._plugin.GetUniqueID() &&
				_config.IsEqual(other._config);
		}
	}
}
