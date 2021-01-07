using MSHC.WPFStub;

namespace AlephNote.PluginInterface.Util
{
    public class UICommand
    {
        public string Header { get; }
        public ITypedCommand<INoteRepository> Command { get; }

        public UICommand(string header, ITypedCommand<INoteRepository> command)
        {
            Header = header;
            Command = command;
        }
    }
}
