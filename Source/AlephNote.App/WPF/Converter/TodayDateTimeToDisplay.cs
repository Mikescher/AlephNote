using AlephNote.WPF.MVVM;
using System;

namespace AlephNote.WPF.Converter
{
	class TodayDateTimeToDisplay : OneWayConverter<DateTime, string>
	{
		protected override string Convert(DateTime value, object parameter)
		{
			var local = value.ToLocalTime();

			return local.ToString("HH:mm:ss");
		}
	}
}
