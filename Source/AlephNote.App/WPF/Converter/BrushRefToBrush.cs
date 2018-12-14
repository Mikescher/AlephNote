using MSHC.WPF.MVVM;
using AlephNote.Common.Themes;
using System.Windows.Media;
using System.Linq;
using System;
using System.Windows;
using AlephNote.WPF.Extensions;

namespace AlephNote.WPF.Converter
{
	class BrushRefToBrush : OneWayConverter<BrushRef, Brush>
	{
		protected override Brush Convert(BrushRef value, object parameter) => Convert(value);

		public static  Brush Convert(BrushRef value)
		{
			if (value.BrushType == BrushRefType.Solid)
			{
				var c = value.GradientSteps.First().Item2;
				var b = new SolidColorBrush(Color.FromArgb(c.A, c.R, c.G, c.B));
				return b;
			}
			else if (value.BrushType == BrushRefType.Gradient_Vertical)
			{
				var b = new LinearGradientBrush()
				{
					StartPoint = new Point(0, 0),
					EndPoint   = new Point(0, 1),
					GradientStops = new GradientStopCollection(value.GradientSteps.Select(p => new GradientStop(p.Item2.ToWCol(), p.Item1))),
				};
				return b;
			}
			else if (value.BrushType == BrushRefType.Gradient_Horizontal)
			{
				var b = new LinearGradientBrush()
				{
					StartPoint = new Point(0, 0),
					EndPoint   = new Point(1, 0),
					GradientStops = new GradientStopCollection(value.GradientSteps.Select(p => new GradientStop(p.Item2.ToWCol(), p.Item1))),
				};
				return b;
			}
			else
			{
				throw new Exception("Unknown BrushRefType: " + value.BrushType);
			}
		}
	}
}
