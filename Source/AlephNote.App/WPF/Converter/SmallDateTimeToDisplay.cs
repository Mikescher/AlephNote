using MSHC.WPF.MVVM;
using System;

namespace AlephNote.WPF.Converter
{
	class SmallDateTimeToDisplay : OneWayConverter<DateTimeOffset, string>
	{
		private static readonly string[] MONTH_LIST = {"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"};

		public SmallDateTimeToDisplay() { }

		protected override string Convert(DateTimeOffset value, object parameter)
		{
			var local = value.ToLocalTime();
			var now = DateTime.Now;

			if (local.DayOfYear == now.DayOfYear && local.Year == now.Year)
			{
				return local.ToString("HH:mm");
			}
			else
			{
				return MONTH_LIST[local.Month - 1] + " " + local.Day;
			}
		}
	}
}
