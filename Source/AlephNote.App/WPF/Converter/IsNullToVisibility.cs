using MSHC.WPF.MVVM;
using System;
using System.Windows;

namespace AlephNote.WPF.Converter
{
	class IsNullToVisibility : OneWayConverter<object, Visibility>
	{
		public IsNullToVisibility() { }

		protected override Visibility Convert(object value, object parameter)
		{
			if (value == null)
				return (Visibility)Enum.Parse(typeof(Visibility), parameter.ToString().Split(';')[0]);
			else
				return (Visibility)Enum.Parse(typeof(Visibility), parameter.ToString().Split(';')[1]);
		}
	}
}
