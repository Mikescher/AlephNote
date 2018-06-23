using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace AlephNote.WPF.Converter
{
	class MultiBoolOperator : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var param = parameter.ToString().Split(';');

			if (values.Any(v=> v == DependencyProperty.UnsetValue)) return DependencyProperty.UnsetValue;

			if (param[0].ToLower() == "or")   return values.Any(v => (bool)v);
			if (param[0].ToLower() == "and")  return values.All(v => (bool)v);
			if (param[0].ToLower() == "nor")  return !values.Any(v => (bool)v);
			if (param[0].ToLower() == "nand") return !values.All(v => (bool)v);

			throw new ArgumentException(param[0]);
		}
		
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
