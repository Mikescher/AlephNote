using AlephNote.Common.Settings;

namespace AlephNote.Common.Shortcuts
{
	public interface IShortcutHandlerParent
	{
		IShortcutHandler GetShortcutHandler();
		AppSettings Settings { get; }
	}
}
