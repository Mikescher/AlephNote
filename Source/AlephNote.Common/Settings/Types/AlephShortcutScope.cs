namespace AlephNote.Common.Settings.Types
{
	public enum AlephShortcutScope
	{
		[EnumDescriptor("", false)]
		None,

		[EnumDescriptor("Whole Window")]
		Window,

		[EnumDescriptor("Notes list")]
		NoteList,

		[EnumDescriptor("Notes edit area")]
		NoteEdit,
		//Global, //TODO OS-global shortcuts
	}
}