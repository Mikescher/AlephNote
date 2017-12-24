using System;

namespace AlephNote.PluginInterface.Objects.AXML
{
	[Flags]
	public enum AXMLSerializationSettings
	{
		None            = 0x00,
		UseEncryption   = 0x01,
		FormattedOutput = 0x02,
		SplittedBase64  = 0x04,
		IncludeTypeInfo = 0x08,
	}
}
