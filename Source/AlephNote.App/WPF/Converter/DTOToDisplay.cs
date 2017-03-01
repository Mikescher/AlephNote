using MSHC.WPF.MVVM;
using System;

namespace AlephNote.WPF.Converter
{
	class DTOToDisplay : OneWayConverter<DateTimeOffset, string>
	{
		public DTOToDisplay() { }

		protected override string Convert(DateTimeOffset value, object parameter)
		{
			return value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
		}
	}
}
