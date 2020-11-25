using System.Collections.Generic;

namespace AlephNote.PluginInterface
{
    public interface INoteRepository
    {
        IEnumerable<INote> EnumerateNotes();
        INote FindNoteByID(string nid);
        void SyncNow();
    }
}
