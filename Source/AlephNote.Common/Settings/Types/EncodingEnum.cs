namespace AlephNote.Common.Settings.Types
{
	public enum EncodingEnum
	{
		[EnumDescriptor("UTF-8 (without BOM)")]
		UTF8,
		
		[EnumDescriptor("UTF-8 (with BOM)")]
		UTF8_BOM,

		[EnumDescriptor("UTF-16")]
		UTF16,

		[EnumDescriptor("UTF-16 (big-endian)")]
		UTF16_BE,

		[EnumDescriptor("UTF-32")]
		UTF32,
	}
}
