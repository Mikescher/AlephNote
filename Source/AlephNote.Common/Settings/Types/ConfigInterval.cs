using MSHC.Lang.Attributes;

namespace AlephNote.Common.Settings.Types
{
	public enum ConfigInterval
	{
		[EnumDescriptor("1 minute")]
		Sync01Min,
		[EnumDescriptor("2 minutes")]
		Sync02Min,
		[EnumDescriptor("5 minutes")]
		Sync05Min,
		[EnumDescriptor("10 minutes")]
		Sync10Min,
		[EnumDescriptor("15 minutes")]
		Sync15Min,
		[EnumDescriptor("30 minutes")]
		Sync30Min,
		[EnumDescriptor("1 hour")]
		Sync01Hour,
		[EnumDescriptor("2 hours")]
		Sync02Hour,
		[EnumDescriptor("3 hours")]
		Sync06Hour,
		[EnumDescriptor("12 hours")]
		Sync12Hour,
		[EnumDescriptor("Manual")]
		SyncManual,
	}
}
