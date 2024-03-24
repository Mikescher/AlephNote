using AlephNote.Common.Plugins;
using AlephNote.Common.Repository;
using AlephNote.Common.Settings;
using AlephNote.Common.Settings.Types;
using AlephNote.Common.Themes;
using AlephNote.Common.Util;
using AlephNote.PluginInterface;
using CommandLine;

namespace AlephNote.GitBackupService;

public class CLIOptions
{
    [Option("settings-path", Required = true)]
    public string SettingsPath { get; set; } = null!;

    [Option("repository-path", Required = true)]
    public string RepoPath { get; set; } = null!;
}

public class Service
{
    private string settingsPath = null!;
    private string repoPath     = null!;
    
    private AppSettings settings      = null!;
    private NoteRepository repository = null!;
    
    public bool Init(string[] args)
    {
        LoggerSingleton.Register(new Logger());
        
        LoggerSingleton.Inst.Info("Service-Init", "Parsing arguments");

       var cliOpts = Parser.Default.ParseArguments<CLIOptions>(args);
       if (cliOpts.Errors.Any())
       {
           foreach (var err in cliOpts.Errors) LoggerSingleton.Inst.Error("Service-Init", err.ToString());
           return false;
       }
       
       this.settingsPath = cliOpts.Value.SettingsPath;
       this.repoPath = cliOpts.Value.RepoPath;
        
        Console.WriteLine();

        LoggerSingleton.Inst.Info("Service-Config", $"Params.SettingsPath                  := {settingsPath}");
        LoggerSingleton.Inst.Info("Service-Config", $"Params.RepoPath                      := {repoPath}");

        if (!File.Exists(settingsPath))
        {
            LoggerSingleton.Inst.Error("Service-Init", $"Settings ${settingsPath} not found");
            return false;
        }
        
        LoggerSingleton.Inst.Info("Service-Init", "Initializing Singletons");
        
        Console.WriteLine();

        PluginManagerSingleton.Register(new PluginMan());
        ThemeManager.Register(new ThemeCache());
        
        Console.WriteLine();

        LoggerSingleton.Inst.Info("Service-Init", "Loading settings");

        try
        {
            settings = AppSettings.Load(settingsPath);
            settings.DisableLogger = false;
            settings.UseRawFolderRepo = false;
            settings.ConflictResolution = ConflictResolutionStrategyConfig.UseServerVersion;
        }
        catch (Exception e)
        {
            LoggerSingleton.Inst.Error("Service", "Failed to load settings", e);
            return false;
        }
        
        Console.WriteLine();
        
        LoggerSingleton.Inst.Info("Service-Config", $"Settings.ActiveAccountID             := {settings.ActiveAccount.ID}");
        LoggerSingleton.Inst.Info("Service-Config", $"Settings.ActiveAccountTitle          := {settings.ActiveAccount.DisplayTitle}");
        LoggerSingleton.Inst.Info("Service-Config", $"Settings.ActivePluginName            := {settings.ActiveAccount.Plugin.GetName()}");
        LoggerSingleton.Inst.Info("Service-Config", $"Settings.ActivePluginID              := {settings.ActiveAccount.Plugin.GetUniqueID()}");
        LoggerSingleton.Inst.Info("Service-Config", $"Settings.ActivePluginVersion         := {settings.ActiveAccount.Plugin.GetVersion()}");

        LoggerSingleton.Inst.Info("Service-Config", $"Settings.SynchronizationFrequency    := {settings.SynchronizationFrequency}");
        
        LoggerSingleton.Inst.Info("Service-Config", $"Settings.ProxyEnabled                := {settings.ProxyEnabled}");
        LoggerSingleton.Inst.Info("Service-Config", $"Settings.ProxyHost                   := {settings.ProxyHost}");
        LoggerSingleton.Inst.Info("Service-Config", $"Settings.ProxyPort                   := {settings.ProxyPort}");
        LoggerSingleton.Inst.Info("Service-Config", $"Settings.ProxyUsername               := {settings.ProxyUsername}");
        LoggerSingleton.Inst.Info("Service-Config", $"Settings.ProxyPassword               := {settings.ProxyPassword}");
        
        LoggerSingleton.Inst.Info("Service-Config", $"Settings.DoGitMirror                 := {settings.DoGitMirror}");
        LoggerSingleton.Inst.Info("Service-Config", $"Settings.GitMirrorPath               := {settings.GitMirrorPath}");
        LoggerSingleton.Inst.Info("Service-Config", $"Settings.GitMirrorFirstName          := {settings.GitMirrorFirstName}");
        LoggerSingleton.Inst.Info("Service-Config", $"Settings.GitMirrorLastName           := {settings.GitMirrorLastName}");
        LoggerSingleton.Inst.Info("Service-Config", $"Settings.GitMirrorMailAddress        := {settings.GitMirrorMailAddress}");
        LoggerSingleton.Inst.Info("Service-Config", $"Settings.GitMirrorDoPush             := {settings.GitMirrorDoPush}");
        LoggerSingleton.Inst.Info("Service-Config", $"Settings.GitMirrorAutoGC             := {settings.GitMirrorAutoGC}");
        LoggerSingleton.Inst.Info("Service-Config", $"Settings.GitMirrorSubfolders         := {settings.GitMirrorSubfolders}");
        
        Console.WriteLine();
        
        LoggerSingleton.Inst.Info("Service-Init", "Creating repository");
        
        repository = new NoteRepository(repoPath, new SyncListener(), settings, settings.ActiveAccount, new Dispatcher());
        
        Console.WriteLine();

        LoggerSingleton.Inst.Info("Service-Init", "Loading and Initializing repository");

        repository.Init();
        
        Console.WriteLine();

        LoggerSingleton.Inst.Info("Service-Config", "Initialized repository");
        
        LoggerSingleton.Inst.Info("Service-Config", $"Repository.ConnectionName            := {repository.ConnectionName}");
        LoggerSingleton.Inst.Info("Service-Config", $"Repository.ConnectionID              := {repository.ConnectionID}");
        LoggerSingleton.Inst.Info("Service-Config", $"Repository.ConnectionDisplayTitle    := {repository.ConnectionDisplayTitle}");
             
        LoggerSingleton.Inst.Info("Service-Config", $"Repository.RepoPath                  := {repository.ProviderID}");
        LoggerSingleton.Inst.Info("Service-Config", $"Repository.PluginName                := {repository.ConnectionName}");
        
        Console.WriteLine();

        return true;
    }
    
    public bool Run()
    {
        LoggerSingleton.Inst.Info("Service-Run", "Starting main loop");

        var shouldExit = false;
        var shouldExitWaitHandle = new ManualResetEvent(false);
        
        Console.CancelKeyPress += (_, e) =>
        {
            LoggerSingleton.Inst.Info("Service-Run", "Received cancel per keypress or sig");
                
            e.Cancel = true;
            shouldExit = true;
            shouldExitWaitHandle.Set();
        };
        
        while (!shouldExit)
        {
            WaitHandle.WaitAny(new WaitHandle[]{shouldExitWaitHandle}, 10*60*1000);
        }
        
        LoggerSingleton.Inst.Info("Service-Run", "Finished main loop - shutting down");
        
        repository.Shutdown();
        
        LoggerSingleton.Inst.Info("Service-Run", "Repository successfully shut down");

        return true;
    }
}