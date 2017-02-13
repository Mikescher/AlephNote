using CommonNote.Settings;
using MSHC.WPF.MVVM;
using System.Windows;

namespace CommonNote.WPF.Converter
{
	class FontModifierToFontStyle : OneWayConverter<FontModifier, FontStyle>
	{
		public FontModifierToFontStyle() { }

		protected override FontStyle Convert(FontModifier value, object parameter)
		{
			return (value == FontModifier.Italic || value == FontModifier.BoldItalic) ? FontStyles.Italic : FontStyles.Normal;
		}
	}
}
