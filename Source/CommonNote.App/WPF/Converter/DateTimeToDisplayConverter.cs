using MSHC.WPF.MVVM;
using System;

namespace CommonNote.WPF.Converter
{
	class DateTimeToDisplayConverter : OneWayConverter<DateTimeOffset, string>
	{
		public DateTimeToDisplayConverter() { }

		protected override string Convert(DateTimeOffset value, object parameter)
		{
			return value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
		}
	}
}
