using System;
using System.Text;

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

	public static class EncodingEnumHelper
	{
		public static Encoding ToEncoding(EncodingEnum e)
		{
			switch (e)
			{
				case EncodingEnum.UTF8:
					return new UTF8Encoding(false);

				case EncodingEnum.UTF8_BOM:
					return new UTF8Encoding(true);

				case EncodingEnum.UTF16:
					return new UnicodeEncoding(false, true);

				case EncodingEnum.UTF16_BE:
					return new UnicodeEncoding(true, true);

				case EncodingEnum.UTF32:
					return new UTF32Encoding();

				default:
					throw new Exception("Invalid value for EncodingEnum " + e);
			}
		}
	}
}
