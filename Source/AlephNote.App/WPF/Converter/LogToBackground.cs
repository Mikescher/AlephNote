using AlephNote.Log;
using MSHC.WPF.MVVM;
using System;
using System.Windows.Media;

namespace AlephNote.WPF.Converter
{
	class LogToBackground : OneWayConverter<LogEventType, Brush>
	{
		public LogToBackground() { }

		private readonly Brush BrushDebug = Brushes.White;
		private readonly Brush BrushInfo  = Brushes.White;
		private readonly Brush BrushWarn  = new SolidColorBrush(Color.FromRgb(255, 255, 128));
		private readonly Brush BrushError = new SolidColorBrush(Color.FromRgb(255, 150, 100));

		protected override Brush Convert(LogEventType value, object parameter)
		{
			switch (value)
			{
				case LogEventType.Debug: return BrushDebug;
				case LogEventType.Information: return BrushInfo;
				case LogEventType.Warning: return BrushWarn;
				case LogEventType.Error: return BrushError;

				default:
					throw new ArgumentOutOfRangeException("value");
			}
		}
	}
}
