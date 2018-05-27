using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace AlephNote.WPF.Converter
{
	class MultiBoolToVisibility : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var param = parameter.ToString().Split(';');

			if (values.Any(v=> v == DependencyProperty.UnsetValue)) return DependencyProperty.UnsetValue;

			var value = false;

			     if (param[0].ToLower() == "or")   value = values.Any(v => (bool)v);
			else if (param[0].ToLower() == "and")  value = values.All(v => (bool)v);
			else if (param[0].ToLower() == "nor")  value = !values.Any(v => (bool)v);
			else if (param[0].ToLower() == "nand") value = !values.All(v => (bool)v);

			if (param.Length < 3)
			{
				return value ? Visibility.Visible : Visibility.Hidden;
			}
			else
			{
				if (value)
					return (Visibility)Enum.Parse(typeof(Visibility), param[1]);
				else
					return (Visibility)Enum.Parse(typeof(Visibility), param[2]);
			}
		}
		
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
