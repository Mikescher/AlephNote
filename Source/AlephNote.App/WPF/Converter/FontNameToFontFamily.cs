using System;
using System.Windows.Controls;
using System.Windows.Media;
using MSHC.WPF.MVVM;

namespace AlephNote.WPF.Converter
{
	public class FontNameToFontFamily : OneWayConverter<string, FontFamily>
	{
		private static readonly FontFamily defaultValue = new TextBlock().FontFamily;
		private static readonly FontFamilyConverter ffc = new FontFamilyConverter();

		private static string _strDefaultValue = null;
		public static string StrDefaultValue => _strDefaultValue ?? (_strDefaultValue = ffc.ConvertToString(defaultValue));

		protected override FontFamily Convert(string value, object parameter)
		{
			if (string.IsNullOrWhiteSpace(value)) return defaultValue;

			try
			{
				return (FontFamily)ffc.ConvertFrom(value);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e);
				return defaultValue;
			}
		}
	}
}
