using MSHC.WPF.MVVM;

namespace AlephNote.PluginInterface.Util
{
    public class UICommand
    {
        public string Header { get; }
        public RelayCommand<INoteRepository> Command { get; }

        public UICommand(string header, RelayCommand<INoteRepository> command)
        {
            Header = header;
            Command = command;
        }
    }
}
