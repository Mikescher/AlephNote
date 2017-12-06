using AlephNote.WPF.MVVM;
using AlephNote.WPF.Util;

namespace AlephNote.WPF.Converter
{
	class PropertyToHelpText : OneWayConverter<string, string>
	{
		protected override string Convert(string value, object parameter)
		{
			return HelpTextsLoader.Get(value);
		}
	}
}
