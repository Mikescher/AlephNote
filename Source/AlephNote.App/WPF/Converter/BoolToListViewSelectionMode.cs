using MSHC.WPF.MVVM;
using System.Windows.Controls;

namespace AlephNote.WPF.Converter
{
	class BoolToListViewSelectionMode : OneWayConverter<bool, SelectionMode>
	{
		protected override SelectionMode Convert(bool value, object parameter) => value ? SelectionMode.Extended : SelectionMode.Single;
	}
}
