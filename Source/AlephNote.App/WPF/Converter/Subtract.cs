using AlephNote.WPF.MVVM;

namespace AlephNote.WPF.Converter
{
	class Subtract : OneWayConverter<double, double>
	{
		public Subtract() { }

		protected override double Convert(double value, object parameter)
		{
			int delta;
			if (!int.TryParse(System.Convert.ToString(parameter), out delta)) delta = 0;

			return value - delta;
		}
	}
}
