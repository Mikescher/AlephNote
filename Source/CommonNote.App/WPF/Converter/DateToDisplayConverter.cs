using MSHC.WPF.MVVM;
using System;

namespace CommonNote.WPF.Converter
{
	class DateToDisplayConverter : OneWayConverter<DateTimeOffset, string>
	{
		public DateToDisplayConverter() { }

		protected override string Convert(DateTimeOffset value, object parameter)
		{
			return value.ToLocalTime().ToString("yyyy-MM-dd");
		}
	}
}
