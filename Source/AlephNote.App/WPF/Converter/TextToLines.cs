using AlephNote.WPF.MVVM;
using System.Text.RegularExpressions;

namespace AlephNote.WPF.Converter
{
	class TextToLines : OneWayConverter<string, int>
	{
		protected override int Convert(string value, object parameter)
		{
			return Regex.Split(value, @"\r?\n").Length;
		}
	}
}
