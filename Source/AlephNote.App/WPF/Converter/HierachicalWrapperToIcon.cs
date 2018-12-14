using MSHC.WPF.MVVM;
using AlephNote.WPF.Util;
using System.Windows;

namespace AlephNote.WPF.Converter
{
	class HierachicalWrapperToIcon : OneWayConverter<HierachicalBaseWrapper, string>
	{
		protected override string Convert(HierachicalBaseWrapper value, object parameter)
		{
			return value.IconSource;
		}
	}
}
