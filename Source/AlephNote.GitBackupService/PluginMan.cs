using AlephNote.Common.Network;
using AlephNote.Common.Plugins;
using AlephNote.Common.Util;
using AlephNote.PluginInterface;
using AlephNote.Plugins.Evernote;
using AlephNote.Plugins.Filesystem;
using AlephNote.Plugins.Headless;
using AlephNote.Plugins.Nextcloud;
using AlephNote.Plugins.SimpleNote;
using AlephNote.Plugins.StandardNote;

namespace AlephNote.GitBackupService;

public class PluginMan: IPluginManager
{
    private readonly IRemotePlugin[] _plugins =
    {
        new HeadlessPlugin(),
        new FilesystemPlugin(),
        new EvernotePlugin(),
        new NextcloudPlugin(),
        new SimpleNotePlugin(),
        new StandardNotePlugin(),
    };

    public IEnumerable<IRemotePlugin> LoadedPlugins => _plugins;

    public PluginMan()
    {
        foreach (var p in _plugins)
        {
            p.Init(LoggerSingleton.Inst);
        }
    }
    
    public void LoadPlugins(string baseDirectory)
    {
       // nop 
    }

    public IRemotePlugin GetDefaultPlugin()
    {
        return _plugins[0];
    }

    public IRemotePlugin? GetPlugin(Guid uuid)
    {
        return LoadedPlugins.FirstOrDefault(p => p.GetUniqueID() == uuid);
    }

    public IProxyFactory GetProxyFactory()
    {
        return new ProxyFactory();
    }
}