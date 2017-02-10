using CommonNote.PluginInterface;
using System;
using System.Net;

namespace CommonNote.Plugins.StandardNote
{
	public class StandardNotePlugin : RemoteBasicProvider
	{
		public static readonly Version Version = new Version(0, 0, 0, 1);
		
		public StandardNotePlugin() : base("Standard Notes", Version, Guid.Parse("30d867a4-cbdc-45c5-950a-c119bf2f2845"))
		{
			//
		}

		public override IRemoteStorageConfiguration CreateEmptyRemoteStorageConfiguration()
		{
			throw new NotImplementedException();
		}

		public override IRemoteStorageConnection CreateRemoteStorageConnection(IWebProxy proxy, IRemoteStorageConfiguration config)
		{
			throw new NotImplementedException();
		}

		public override INote CreateEmptyNode()
		{
			throw new NotImplementedException();
		}
	}
}
