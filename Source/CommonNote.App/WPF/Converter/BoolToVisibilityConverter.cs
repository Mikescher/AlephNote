using MSHC.WPF.MVVM;
using System;
using System.Windows;

namespace CommonNote.WPF.Converter
{
	class BoolToVisibilityConverter : OneWayConverter<bool, Visibility>
	{
		public BoolToVisibilityConverter() { }

		protected override Visibility Convert(bool value, object parameter)
		{
			if (value)
				return (Visibility)Enum.Parse(typeof(Visibility), parameter.ToString().Split(';')[0]);
			else
				return (Visibility)Enum.Parse(typeof(Visibility), parameter.ToString().Split(';')[1]);
		}
	}
}
