using MSHC.Lang.Attributes;

namespace AlephNote.Common.Settings.Types
{
	public enum ListHighlightMode
	{
		[EnumDescriptor("Disabled")]
		Disabled,

		[EnumDescriptor("On notes tagged [list]")]
		WithTag,

		[EnumDescriptor("Always")]
		Always,
	}
}
