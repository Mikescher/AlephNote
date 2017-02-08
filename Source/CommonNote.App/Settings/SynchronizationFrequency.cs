using System.ComponentModel;

namespace CommonNote.Settings
{
	public enum SynchronizationFrequency
	{
		[Description("1 minute")]
		Sync01Min,
		[Description("2 minutes")]
		Sync02Min,
		[Description("5 minutes")]
		Sync05Min,
		[Description("10 minutes")]
		Sync10Min,
		[Description("15 minutes")]
		Sync15Min,
		[Description("30 minutes")]
		Sync30Min,
		[Description("1 hour")]
		Sync01Hour,
		[Description("2 hours")]
		Sync02Hour,
		[Description("3 hours")]
		Sync06Hour,
		[Description("12 hours")]
		Sync12Hour,
	}
}
