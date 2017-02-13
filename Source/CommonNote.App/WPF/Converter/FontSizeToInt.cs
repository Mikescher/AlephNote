using CommonNote.Settings;
using MSHC.WPF.MVVM;

namespace CommonNote.WPF.Converter
{
	class FontSizeToInt : OneWayConverter<FontSize, int>
	{
		public FontSizeToInt() { }

		protected override int Convert(FontSize value, object parameter)
		{
			return (int) value;
		}
	}
}
