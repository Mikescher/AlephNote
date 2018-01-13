using AlephNote.WPF.MVVM;
using System;
using System.Windows;

namespace AlephNote.WPF.Converter
{
	class BoolToVisibility : OneWayConverter<bool, Visibility>
	{
		protected override Visibility Convert(bool value, object parameter)
		{
			if (string.IsNullOrWhiteSpace(parameter?.ToString()))
			{
				return value ? Visibility.Visible : Visibility.Hidden;
			}
			else
			{
				if (value)
					return (Visibility)Enum.Parse(typeof(Visibility), parameter.ToString().Split(';')[0]);
				else
					return (Visibility)Enum.Parse(typeof(Visibility), parameter.ToString().Split(';')[1]);
			}

		}
	}
}
