using MSHC.Lang.Attributes;

namespace AlephNote.Common.Settings.Types
{
	public enum FocusTarget
	{
		[EnumDescriptor("Unchanged")]
		Unchanged,

		[EnumDescriptor("Note title")]
		NoteTitle,

		[EnumDescriptor("Note tags")]
		NoteTags,

		[EnumDescriptor("Note content")]
		NoteText,

		[EnumDescriptor("Note sidebar")]
		NoteList,

		[EnumDescriptor("Folder sidebar")]
		FolderList,
	}
}
