using AlephNote.Common.Settings.Types;
using MSHC.WPF.MVVM;
using System.Windows;

namespace AlephNote.WPF.Converter
{
	class FontModifierToFontWeight : OneWayConverter<FontModifier, FontWeight>
	{
		protected override FontWeight Convert(FontModifier value, object parameter)
		{
			return (value == FontModifier.Bold || value == FontModifier.BoldItalic) ? FontWeights.Bold : FontWeights.Normal;
		}
	}
}
