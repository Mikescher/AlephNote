using AlephNote.WPF.MVVM;
using System;

namespace AlephNote.WPF.Converter
{
	class DateOnlyToDisplay : OneWayConverter<DateTimeOffset, string>
	{
		public DateOnlyToDisplay() { }

		protected override string Convert(DateTimeOffset value, object parameter)
		{
			var local = value.ToLocalTime();

			return local.ToLocalTime().ToString("yyyy-MM-dd");
		}
	}
}
