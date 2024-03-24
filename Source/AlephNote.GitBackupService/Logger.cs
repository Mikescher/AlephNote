using AlephNote.PluginInterface;

namespace AlephNote.GitBackupService;

public class Logger: AlephLogger
{
    public override void Trace(string src, string text, string? longtext = null)
    {
        ColorConsole.Out.WriteLineGray(longtext == null ? $"[INF] <{src}> {text}" : $"[INF] <{src}> {text}\n{longtext}");
    }

    public override void TraceExt(string src, string text, params (string, string)[] longtexts)
    {
        var pad = longtexts.Length == 0 ? 0 : longtexts.Max(l => l.Item1.Length);
        ColorConsole.Out.WriteLineGray($"[INF] <{src}> {text}\n{string.Join("\n", longtexts.Select(txt => txt.Item1.PadRight(pad, ' ') + " = " + txt.Item2))}");
    }

    public override void Debug(string src, string text, string? longtext = null)
    {
        ColorConsole.Out.WriteLineGray(longtext == null ? $"[DBG] <{src}> {text}" : $"[DBG] <{src}> {text}\n{longtext}");
    }

    public override void Info(string src, string text, string? longtext = null)
    {
        ColorConsole.Out.WriteLineCyan(longtext == null ? $"[INF] <{src}> {text}" : $"[INF] <{src}> {text}\n{longtext}");
    }

    public override void Warn(string src, string text, string? longtext = null)
    {
        ColorConsole.Out.WriteLineYellow(longtext == null ? $"[WRN] <{src}> {text}" : $"[WRN] <{src}> {text}\n{longtext}");
    }

    public override void Error(string src, string text, string? longtext = null)
    {
        ColorConsole.Error.WriteLineRed(longtext == null ? $"[ERR] <{src}> {text}" : $"[ERR] <{src}> {text}\n{longtext}");
    }

    public override void Error(string src, string text, Exception? e)
    {
        ColorConsole.Error.WriteLineRed(e == null ? $"[ERR] <{src}> {text}" : $"[ERR] <{src}> {text}\n{e}");
    }

    public override void ShowExceptionDialog(string title, Exception? e)
    {
        ColorConsole.Error.WriteLineRed(e==null ?  $"[EXC] <Exception> {title}" : $"[EXC] <Exception> {title}\n{e}");
    }

    public override void ShowExceptionDialog(string title, Exception e, string additionalInfo)
    {
        ColorConsole.Error.WriteLineRed(additionalInfo=="" ? $"[EXC] <Exception> {title}\n{e}" : $"[EXC] <Exception> {title}\n{e}\n{additionalInfo}");
    }

    public override void ShowExceptionDialog(string title, string message, Exception e, params Exception[] additionalExceptions)
    {
        ColorConsole.Error.WriteLineRed(additionalExceptions.Length==0 ? $"[EXC] <Exception> {title}\n{message}\n{e}" : $"[EXC] <Exception> {title}\n{message}\n{e}\n{string.Join("\n", additionalExceptions.Select(p => p.ToString()))}");
    }

    public override void ShowSyncErrorDialog(string message, string trace)
    {
        ColorConsole.Error.WriteLineRed($"[SYN] <Sync-Error>: {message}\n{trace}");
    }

    public override void ShowSyncErrorDialog(string message, Exception e)
    {
        ColorConsole.Error.WriteLineRed($"[SYN] <Sync-Error>: {message}\n{e}");
    }
}