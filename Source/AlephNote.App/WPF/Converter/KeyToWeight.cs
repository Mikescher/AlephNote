using AlephNote.WPF.MVVM;
using System.Windows;
using AlephNote.Common.Settings.Types;

namespace AlephNote.WPF.Converter
{
	class KeyToWeight : OneWayConverter<AlephKey, FontWeight>
	{
		protected override FontWeight Convert(AlephKey value, object parameter)
		{
			return (value == AlephKey.None) ? FontWeights.Normal : FontWeights.Bold;
		}
	}
}
