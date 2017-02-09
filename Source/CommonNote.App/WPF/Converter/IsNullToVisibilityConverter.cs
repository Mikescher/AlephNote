using MSHC.WPF.MVVM;
using System;
using System.Windows;

namespace CommonNote.WPF.Converter
{
	class IsNullToVisibilityConverter : OneWayConverter<object, Visibility>
	{
		public IsNullToVisibilityConverter() { }

		protected override Visibility Convert(object value, object parameter)
		{
			if (value == null)
				return (Visibility)Enum.Parse(typeof(Visibility), parameter.ToString().Split(';')[0]);
			else
				return (Visibility)Enum.Parse(typeof(Visibility), parameter.ToString().Split(';')[1]);
		}
	}
}
