using AlephNote.PluginInterface;
using MSHC.Util.Helper;

namespace AlephNote.GitBackupService;

public class Logger: AlephLogger
{
    private object fileLock = new object();
    
    public string? LogPath = null;
    public bool SendSCN = false;
    
    public override void Trace(string src, string text, string? longtext = null)
    {
        ColorConsole.Out.WriteLineGray($"[TRC] <{src}> {text}");
        this.WriteLog("TRACE", src, text, longtext);
    }

    public override void TraceExt(string src, string text, params (string, string)[] longtexts)
    {
        var pad = longtexts.Length == 0 ? 0 : longtexts.Max(l => l.Item1.Length);
        ColorConsole.Out.WriteLineGray($"[TRC] <{src}> {text}\n");
        
        this.WriteLog("TRACE", src, text, longtexts.Length==0 ? null : string.Join("\n", longtexts.Select(txt => txt.Item1.PadRight(pad, ' ') + " = " + txt.Item2)));
    }

    public override void Debug(string src, string text, string? longtext = null)
    {
        ColorConsole.Out.WriteLineGray($"[DBG] <{src}> {text}");
        this.WriteLog("DEBUG", src, text, longtext);
    }

    public override void Info(string src, string text, string? longtext = null)
    {
        ColorConsole.Out.WriteLineCyan($"[INF] <{src}> {text}");
        this.WriteLog("INFO", src, text, longtext);
    }

    public override void Warn(string src, string text, string? longtext = null)
    {
        ColorConsole.Out.WriteLineYellow($"[WRN] <{src}> {text}");
        this.WriteLog("WARN", src, text, longtext);
    }

    public override void Error(string src, string text, string? longtext = null)
    {
        ColorConsole.Error.WriteLineRed($"[ERR] <{src}> {text}");
        this.WriteLog("ERROR", src, text, longtext);
        this.SendNotification(src, text, longtext);
    }

    public override void Error(string src, string text, Exception? e)
    {
        ColorConsole.Error.WriteLineRed($"[ERR] <{src}> {text}");
        this.WriteLog("ERROR", src, text, e == null ? null : e.Message+"\n"+e.ToString());
        this.SendNotification(src, text, e == null ? null : e.Message + "\n" + e.ToString());
    }

    public override void ShowExceptionDialog(string title, Exception? e)
    {
        ColorConsole.Error.WriteLineRed(e==null ? $"[EXC] <Exception> {title}" : $"[EXC] <Exception> {title}: {e.Message}");
        this.WriteLog("EXCEPTION", "Dialog", title, e==null ? null : e.Message+"\n"+e.ToString());
        this.SendNotification("Dialog", title, e==null ? null : e.Message+"\n"+e.ToString());
    }

    public override void ShowExceptionDialog(string title, Exception? e, string additionalInfo)
    {
        ColorConsole.Error.WriteLineRed(e==null ? $"[EXC] <Exception> {title}" : $"[EXC] <Exception> {title}: {e.Message}");
        this.WriteLog("EXCEPTION", "Dialog", title, e==null ? additionalInfo : e.Message+"\n"+e.ToString()+(additionalInfo== "" ? "" : $"\n{additionalInfo}"));
        this.SendNotification("Dialog", title, e==null ? additionalInfo : e.Message+"\n"+e.ToString()+(additionalInfo== "" ? "" : $"\n{additionalInfo}"));
    }

    public override void ShowExceptionDialog(string title, string message, Exception? e, params Exception[] additionalExceptions)
    {
        ColorConsole.Error.WriteLineRed(e==null ? $"[EXC] <Exception> {title}" : $"[EXC] <Exception> {title}: {e.Message}");
        this.WriteLog("EXCEPTION", "Dialog", title, string.Join("\n", Enumerable.Empty<Exception?>().Append(e).Concat(additionalExceptions).Where(p => p != null).Select(p => p!.Message+"\n"+p)));
        this.SendNotification("Dialog", message, string.Join("\n", Enumerable.Empty<Exception?>().Append(e).Concat(additionalExceptions).Where(p => p != null).Select(p => p!.Message+"\n"+p)));
    }

    public override void ShowSyncErrorDialog(string message, string trace)
    {
        ColorConsole.Error.WriteLineRed($"[SYN] <Sync-Error>: {message}\n{trace}");
        this.WriteLog("SYNC-ERROR", "Dialog", message, trace);
        this.SendNotification("Sync", message, trace);
    }

    public override void ShowSyncErrorDialog(string message, Exception? e)
    {
        ColorConsole.Error.WriteLineRed($"[SYN] <Sync-Error>: {message}\n{e}");
        this.WriteLog("SYNC-ERROR", "Dialog", message, e==null ? null : e.Message+"\n"+e.ToString());
        this.SendNotification("Sync", message, e==null ? null : e.Message+"\n"+e.ToString());
    }

    private void WriteLog(string type, string source, string message, string? extra)
    {
        var path = this.LogPath;
        if (path == null) return;

        var txt = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}   [{type}] @ {source}: {message}";
        if (!string.IsNullOrEmpty(extra)) txt += "\n" + extra;

        txt += "\n\n";

        (new Thread(() =>
        {
            try
            {
                lock (fileLock)
                {
                    File.AppendAllText(path, txt);
                }
            }
            catch (Exception e)
            {
                ColorConsole.Error.WriteLineRed($"[!!!] <Write-Log>: Failed to write to logfile\n{e}");
            }
        })).Start();
        
    }

    private void SendNotification(string source, string message, string? content)
    {
        if (!SendSCN) return;

        content = content ?? "";
        if (content.Length > 2040) content = content[..2040] + "...";

        var title = $"[{source}] {message}";
        if (title.Length >= 120)
        {
            title = title[..115] + "...";
            content = message + "\n\n" + content;
            
            if (content.Length > 2040) content = content[..2040] + "...";
        }

        (new Thread(() =>
        {
            
            try
            {
                var r = ProcessHelper.ProcExecute("scnsend", new[]
                {
                    "@AN_GITSYNC", // channel
                    title,         // title
                    content,       // content
                    "1"            // priority
                });

                if (r.ExitCode != 0)
                {
                    ColorConsole.Error.WriteLineRed($"[!!!] <Send-SCN>: Failed to send to scn\n{r.StdCombined}");
                }
                
            }
            catch (Exception e)
            {
                ColorConsole.Error.WriteLineRed($"[!!!] <Send-SCN>: Failed to send to scn\n{e}");
            }
            
        })).Start();
    }

}