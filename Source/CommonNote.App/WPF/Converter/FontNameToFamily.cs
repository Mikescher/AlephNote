using MSHC.WPF.MVVM;
using System.Windows.Media;

namespace CommonNote.WPF.Converter
{
	class FontNameToFamily : OneWayConverter<string, FontFamily>
	{
		public FontNameToFamily() { }

		protected override FontFamily Convert(string value, object parameter)
		{
			return new FontFamily(value);
		}
	}
}
