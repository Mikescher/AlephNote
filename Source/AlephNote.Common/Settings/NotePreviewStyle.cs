using AlephNote.Common.Settings;

namespace AlephNote.Settings
{
	public enum NotePreviewStyle
	{
		[EnumDescriptor("Simple one line")]
		Simple,

		[EnumDescriptor("One line with date")]
		Extended,

		[EnumDescriptor("Multiple lines with preview")]
		FullPreview,
	}
}
