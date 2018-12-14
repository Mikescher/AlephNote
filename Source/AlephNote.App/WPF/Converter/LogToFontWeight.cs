using AlephNote.Log;
using MSHC.WPF.MVVM;
using System;
using System.Windows;

namespace AlephNote.WPF.Converter
{
	class LogToFontWeight : OneWayConverter<LogEventType, FontWeight>
	{
		protected override FontWeight Convert(LogEventType value, object parameter)
		{
			switch (value)
			{
				case LogEventType.Trace:       return FontWeights.Normal;
				case LogEventType.Debug:       return FontWeights.Normal;
				case LogEventType.Information: return FontWeights.Normal;
				case LogEventType.Warning:     return FontWeights.Normal;
				case LogEventType.Error:       return FontWeights.Bold;

				default:
					throw new ArgumentOutOfRangeException(nameof(value));
			}
		}
	}
}
