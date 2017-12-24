using AlephNote.WPF.MVVM;
using System;
using System.Windows;

namespace AlephNote.WPF.Converter
{
	class StringIsNotWhitespace : OneWayConverter<string, bool>
	{
		protected override bool Convert(string value, object parameter)
		{
			return !string.IsNullOrWhiteSpace(value);
		}
	}
}
