namespace AlephNote.Common.Settings.Types
{
	public enum URLMatchingMode
	{
		[EnumDescriptor("Standard-conform")]
		StandardConform,
		
		[EnumDescriptor("Extended character set")]
		ExtendedMatching,

		[EnumDescriptor("Tolerant")]
		Tolerant,
	}
}
