using MSHC.WPF.MVVM;
using System.Windows.Media;
using AlephNote.Common.Settings.Types;

namespace AlephNote.WPF.Converter
{
	class KeyToColor : OneWayConverter<AlephKey, Brush>
	{
		private static readonly Brush B_ON  = Brushes.Black;
		private static readonly Brush B_OFF = new SolidColorBrush(Color.FromRgb(96, 96, 96));

		protected override Brush Convert(AlephKey value, object parameter)
		{
			return (value == AlephKey.None) ? B_OFF : B_ON;
		}
	}
}
