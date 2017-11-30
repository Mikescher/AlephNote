using AlephNote.WPF.MVVM;
using System.Windows;
using System.Windows.Media;
using AlephNote.Common.Settings.Types;

namespace AlephNote.WPF.Converter
{
	class KeyToColor : OneWayConverter<AlephKey, Brush>
	{
		public static readonly Brush B_ON  = Brushes.Black;
		public static readonly Brush B_OFF = new SolidColorBrush(Color.FromRgb(96, 96, 96));

		public KeyToColor() { }

		protected override Brush Convert(AlephKey value, object parameter)
		{
			return (value == AlephKey.None) ? B_OFF : B_ON;
		}
	}
}
