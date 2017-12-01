using AlephNote.WPF.MVVM;

namespace AlephNote.WPF.Converter
{
	class NegateBool : OneWayConverter<bool, bool>
	{
		public NegateBool() { }

		protected override bool Convert(bool value, object parameter)
		{
			return !value;
		}
	}
}
