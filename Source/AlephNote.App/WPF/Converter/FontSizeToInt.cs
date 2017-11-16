using AlephNote.Common.Settings.Types;
using AlephNote.WPF.MVVM;

namespace AlephNote.WPF.Converter
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
