using CommonNote.PluginInterface;
using System;

namespace CommonNote.Plugins.LocalStorage
{
    public class LocalStoragePlugin : ICommonNoteProvider
    {
	    public string GetName()
	    {
		    return "Local Storage";
	    }

	    public Version GetVersion()
	    {
		    throw new NotImplementedException();
	    }

	    public IRemoteStorageConfiguration CreateEmptyRemoteStorageConfiguration()
	    {
		    throw new NotImplementedException();
	    }

	    public IRemoteStorageConnection CreateRemoteStorageConnection(IRemoteStorageConfiguration config)
	    {
		    throw new NotImplementedException();
	    }
    }
}
