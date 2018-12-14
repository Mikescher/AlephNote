using MSHC.WPF.MVVM;
using System;
using System.Windows;

namespace AlephNote.WPF.Converter
{
	class EmptyToVisibility : OneWayConverter<string, Visibility>
	{
		protected override Visibility Convert(string value, object parameter)
		{
			if (string.IsNullOrWhiteSpace(parameter?.ToString()))
			{
				return string.IsNullOrEmpty(value) ? Visibility.Visible : Visibility.Hidden;
			}
			else
			{
				if (string.IsNullOrEmpty(value))
					return (Visibility)Enum.Parse(typeof(Visibility), parameter.ToString().Split(';')[0]);
				else
					return (Visibility)Enum.Parse(typeof(Visibility), parameter.ToString().Split(';')[1]);
			}

		}
	}
}
