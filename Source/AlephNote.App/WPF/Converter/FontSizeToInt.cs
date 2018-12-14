using AlephNote.Common.Settings.Types;
using MSHC.WPF.MVVM;

namespace AlephNote.WPF.Converter
{
	class FontSizeToInt : OneWayConverter<FontSize, int>
	{
		protected override int Convert(FontSize value, object parameter)
		{
			return (int) value;
		}
	}
}
