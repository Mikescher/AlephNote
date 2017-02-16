using System.ComponentModel;

namespace AlephNote.Settings
{
	public enum NotePreviewStyle
	{
		[Description("Simple one line")]
		Simple,

		[Description("One line with date")]
		Extended,

		[Description("Multiple lines with preview")]
		FullPreview,
	}
}
