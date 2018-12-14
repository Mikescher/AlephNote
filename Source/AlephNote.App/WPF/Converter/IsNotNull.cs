using MSHC.WPF.MVVM;

namespace AlephNote.WPF.Converter
{
	class IsNotNull : OneWayConverter<object, bool>
	{
		protected override bool Convert(object value, object parameter)
		{
			return (value != null);
		}
	}
}
