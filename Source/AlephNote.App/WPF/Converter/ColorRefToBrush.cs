using AlephNote.WPF.MVVM;
using AlephNote.Common.Themes;
using System.Windows.Media;

namespace AlephNote.WPF.Converter
{
	class ColorRefToBrush : OneWayConverter<ColorRef, Brush>
	{
		protected override Brush Convert(ColorRef value, object parameter)
		{
			return new SolidColorBrush(Color.FromArgb(value.A, value.R, value.G, value.B));
		}
	}
}
