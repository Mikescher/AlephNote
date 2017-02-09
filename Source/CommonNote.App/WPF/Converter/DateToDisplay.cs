using MSHC.WPF.MVVM;
using System;

namespace CommonNote.WPF.Converter
{
	class DateToDisplay : OneWayConverter<DateTimeOffset, string>
	{
		public DateToDisplay() { }

		protected override string Convert(DateTimeOffset value, object parameter)
		{
			return value.ToLocalTime().ToString("yyyy-MM-dd");
		}
	}
}
