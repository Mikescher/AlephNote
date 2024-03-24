using AlephNote.Common.Settings;
using AlephNote.Common.Util;
using AlephNote.PluginInterface;
using AlephNote.PluginInterface.AppContext;

namespace AlephNote.GitBackupService;

public class ServiceContext : IAlephAppContext
{
    private AppSettings _settings;

    public ServiceContext(AppSettings settings)
    {
        _settings = settings;
    }

    public IReadonlyAlephSettings GetSettings()
    {
        return _settings;
    }

    public AlephLogger GetLogger()
    {
        return LoggerSingleton.Inst;
    }

    public Tuple<int, int, int, int> GetAppVersion()
    {
        return Tuple.Create(1, 0, 1, 0);
    }

    public bool IsDebugMode { get; set; } = false;
}