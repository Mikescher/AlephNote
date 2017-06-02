using AlephNote.WPF.MVVM;
using System.Windows;

namespace AlephNote.WPF.Converter
{
	class BoolToFontWeight : OneWayConverter<bool, FontWeight>
	{
		public BoolToFontWeight() { }

		protected override FontWeight Convert(bool value, object parameter)
		{
			if (value)
				return FontWeights.Bold;
			else
				return FontWeights.Normal;
		}
	}
}
