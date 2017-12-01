using AlephNote.WPF.MVVM;
using System.Windows.Media;

namespace AlephNote.WPF.Converter
{
	class ProgressIndicatorConverter : OneWayConverter<int, Brush>
	{
		private static readonly SolidColorBrush _brushBlue = new SolidColorBrush(Color.FromRgb(52, 152, 219));

		protected override Brush Convert(int progress, object parameter)
		{
			string config = (parameter ?? "").ToString();
			int position = int.Parse(config.Split(';')[0]);
			int count = int.Parse(config.Split(';')[1]);

			if (progress < -80)
				return Brushes.Lime;
			if (progress < 0)
				return Brushes.Tomato;

			progress = progress % (count * 2);

			if (progress <= count) // >>
			{
				if (progress <= position)
					return Brushes.WhiteSmoke;
				else
					return _brushBlue;
			}
			else // <<
			{
				if (progress - count > position)
					return Brushes.WhiteSmoke;
				else
					return _brushBlue;
			}
		}
	}
}