using System;
using AlephNote.WPF.MVVM;

namespace AlephNote.WPF.Converter
{
	class EnumValueToBoolean : OneWayConverter<object, bool>
	{
		protected override bool Convert(object value, object parameter)
		{
			return String.Equals(value.ToString(), (parameter ?? "").ToString(), StringComparison.CurrentCultureIgnoreCase);
		}
	}
}
