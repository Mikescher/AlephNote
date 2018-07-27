using System.Windows;
using System.Windows.Data;
using AlephNote.WPF.MVVM;

namespace AlephNote.WPF.Converter
{
	[ValueConversion(typeof(bool), typeof(Visibility))]
	class InvertedBoolToVisibility : OneWayConverter<bool, Visibility>
	{
		protected override Visibility Convert(bool value, object parameter) => value ? Visibility.Collapsed : Visibility.Visible;
	}
}
