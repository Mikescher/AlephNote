namespace AlephNote.Common.Settings.Types
{
	public enum LinkHighlightMode
	{
		[EnumDescriptor("Disabled")]
		Disabled,

		[EnumDescriptor("Highlight only")]
		OnlyHighlight,

		[EnumDescriptor("Clickable (single click)")]
		SingleClick,

		[EnumDescriptor("Clickable (double click)")]
		DoubleClick,

		[EnumDescriptor("Clickable (ctrl + click)")]
		ControlClick,
	}
}
