using CommonNote.Settings;
using MSHC.WPF.MVVM;
using System.Windows;

namespace CommonNote.WPF.Converter
{
	class FontModifierToFontWeight : OneWayConverter<FontModifier, FontWeight>
	{
		public FontModifierToFontWeight() { }

		protected override FontWeight Convert(FontModifier value, object parameter)
		{
			return (value == FontModifier.Bold || value == FontModifier.BoldItalic) ? FontWeights.Bold : FontWeights.Normal;
		}
	}
}
