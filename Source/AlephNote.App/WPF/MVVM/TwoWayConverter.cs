using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace AlephNote.WPF.MVVM
{
	public abstract class TwoWayConverter<TSource, TTarget> : MarkupExtension, IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is TSource))
			{
				// ReSharper disable once ExpressionIsAlwaysNull
				if (value == null && typeof (TSource).IsClass) return Convert((TSource)value, parameter);

				return DependencyProperty.UnsetValue;
			}

			return Convert((TSource)value, parameter);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is TTarget))
			{
				// ReSharper disable once ExpressionIsAlwaysNull
				if (value == null && typeof(TTarget).IsClass) return ConvertBack((TTarget)value, parameter);

				return DependencyProperty.UnsetValue;
			}

			return ConvertBack((TTarget)value, parameter);
		}

		protected abstract TTarget Convert(TSource value, object parameter);
		protected abstract TSource ConvertBack(TTarget value, object parameter);
		
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return this;
		}
	}
}
