using AlephNote.WPF.MVVM;
using System;

namespace AlephNote.WPF.Converter
{
	class DateTimeToDisplay : OneWayConverter<DateTime, string>
	{
		public DateTimeToDisplay() { }

		protected override string Convert(DateTime value, object parameter)
		{
			return value.ToString("yyyy-MM-dd HH:mm:ss");
		}
	}
}
