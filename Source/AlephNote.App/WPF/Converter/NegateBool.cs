using MSHC.WPF.MVVM;

namespace AlephNote.WPF.Converter
{
	class NegateBool : OneWayConverter<bool, bool>
	{
		protected override bool Convert(bool value, object parameter) => !value;
	}
}
