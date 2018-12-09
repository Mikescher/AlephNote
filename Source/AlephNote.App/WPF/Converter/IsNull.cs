using AlephNote.WPF.MVVM;

namespace AlephNote.WPF.Converter
{
	class IsNull : OneWayConverter<object, bool>
	{
		protected override bool Convert(object value, object parameter)
		{
			return (value == null);
		}
	}
}
