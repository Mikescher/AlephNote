using AlephNote.Common.Repository;

namespace AlephNote.GitBackupService;

public class Dispatcher: IAlephDispatcher
{
    public IDisposable EnableCustomDispatcher()
    {
        return new DummyDisposable();
    }

    public void BeginInvoke(Action a)
    {
        a();
    }

    public void Invoke(Action a)
    {
        a();
    }

    public void Work()
    {
        // nop
    }
}