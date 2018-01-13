using AlephNote.Log;
using AlephNote.WPF.MVVM;
using System;
using System.Windows.Media;

namespace AlephNote.WPF.Converter
{
	class LogToBackground : OneWayConverter<LogEventType, Brush>
	{
		private readonly Brush _brushTrace = Brushes.White;
		private readonly Brush _brushDebug = Brushes.White;
		private readonly Brush _brushInfo  = Brushes.White;
		private readonly Brush _brushWarn  = new SolidColorBrush(Color.FromRgb(255, 255, 128));
		private readonly Brush _brushError = new SolidColorBrush(Color.FromRgb(255, 150, 100));

		protected override Brush Convert(LogEventType value, object parameter)
		{
			switch (value)
			{
				case LogEventType.Trace:       return _brushTrace;
				case LogEventType.Debug:       return _brushDebug;
				case LogEventType.Information: return _brushInfo;
				case LogEventType.Warning:     return _brushWarn;
				case LogEventType.Error:       return _brushError;

				default:
					throw new ArgumentOutOfRangeException(nameof(value));
			}
		}
	}
}
