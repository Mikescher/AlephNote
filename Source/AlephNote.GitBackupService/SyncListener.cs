using AlephNote.Common.Repository;
using AlephNote.Common.Util;
using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Util;

namespace AlephNote.GitBackupService;

public class SyncListener: ISynchronizationFeedback
{
    public void StartSync()
    {
        LoggerSingleton.Inst.Info("Sync", "Sync started");
    }

    public void SyncSuccess(DateTimeOffset now)
    {
        LoggerSingleton.Inst.Info("Sync", "Sync finished (success)");
    }

    public void SyncError(List<Tuple<string, Exception>> errors)
    {
        LoggerSingleton.Inst.Error("Sync", "Sync finished with errors", string.Join(Environment.NewLine, errors.Select(p => p.Item1 + "\n" + p.Item2)));
    }

    public void OnSyncRequest()
    {
        LoggerSingleton.Inst.Info("Sync", "Sync requested");
    }

    public void OnNoteChanged(NoteChangedEventArgs e)
    {
        LoggerSingleton.Inst.Warn("Sync", $"Note [{e.Note.UniqueName}] '{e.Note.Title}' was changed ({e.PropertyName})");
    }

    public void ShowConflictResolutionDialog(string uuid, string txt0, string ttl0, List<string> tgs0, DirectoryPath ndp0, string txt1, string ttl1, List<string> tgs1, DirectoryPath ndp1)
    {
        LoggerSingleton.Inst.Error("Conflict", $"Cannot show conflict dialog for note {uuid}");
    }
}