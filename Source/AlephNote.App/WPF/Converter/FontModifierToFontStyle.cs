using AlephNote.Common.Settings.Types;
using MSHC.WPF.MVVM;
using System.Windows;

namespace AlephNote.WPF.Converter
{
	class FontModifierToFontStyle : OneWayConverter<FontModifier, FontStyle>
	{
		protected override FontStyle Convert(FontModifier value, object parameter)
		{
			return (value == FontModifier.Italic || value == FontModifier.BoldItalic) ? FontStyles.Italic : FontStyles.Normal;
		}
	}
}
