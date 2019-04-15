using MSHC.Lang.Attributes;

namespace AlephNote.Common.Settings.Types
{
	public enum SearchDelayMode
	{
		[EnumDescriptor("Automatically determine mode")]
		Auto,

		[EnumDescriptor("Search while typing")]
		Direct,

		[EnumDescriptor("Search when stopped typing")]
		Delayed,

		[EnumDescriptor("Search when pressing Enter")]
		Manual,
	}
}
