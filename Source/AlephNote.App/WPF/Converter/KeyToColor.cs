using AlephNote.WPF.MVVM;
using System.Windows;
using System.Windows.Media;
using AlephNote.Common.Settings.Types;

namespace AlephNote.WPF.Converter
{
	class KeyToColor : OneWayConverter<AlephKey, Color>
	{
		public KeyToColor() { }

		protected override Color Convert(AlephKey value, object parameter)
		{
			return (value == AlephKey.None) ? Colors.DarkGray : Colors.Black;
		}
	}
}
