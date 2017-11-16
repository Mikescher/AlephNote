namespace AlephNote.Common.Settings.Types
{
	public enum MarkdownHighlightMode
	{
		[EnumDescriptor("Disabled")]
		Disabled,

		[EnumDescriptor("On notes tagged [markdown]")]
		WithTag,

		[EnumDescriptor("Always")]
		Always,
	}
}
