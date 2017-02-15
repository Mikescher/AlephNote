using System.ComponentModel;

namespace AlephNote.Settings
{
	public enum FontModifier
	{
		[Description("Normal")]
		Normal,

		[Description("Bold")]
		Bold,

		[Description("Italic")]
		Italic,

		[Description("Bold Italic")]
		BoldItalic,
	}
}
