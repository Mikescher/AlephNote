using MSHC.WPF.MVVM;
using System;

namespace AlephNote.WPF.Converter
{
	class SmartDateTimeToDisplay : OneWayConverter<DateTimeOffset, string>
	{
		protected override string Convert(DateTimeOffset value, object parameter)
		{
			var local = value.ToLocalTime();
			var now = DateTime.Now;

			if (local.DayOfYear == now.DayOfYear && local.Year == now.Year) 
				return local.ToString("HH:mm:ss");
			else
				return local.ToString("yyyy-MM-dd");
		}
	}
}
