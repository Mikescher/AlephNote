using AlephNote.WPF.MVVM;
using System.Windows.Media;
using AlephNote.Common.Settings;

namespace AlephNote.WPF.Converter
{
	class TagToBorderBrush : OneWayConverter<string, Brush>
	{
		private static readonly Brush HighlightBrush = new DrawingBrush
		{
			Viewport = new System.Windows.Rect(0, 0, 8, 8),
			ViewportUnits = BrushMappingMode.Absolute,
			TileMode = TileMode.Tile,
			Drawing = new GeometryDrawing
			{
				Brush = Brushes.Black,
				Geometry = new GeometryGroup
				{
					Children = new GeometryCollection(new Geometry[]
					{
						new RectangleGeometry { Rect = new System.Windows.Rect(0,  0,  50, 50) },
						new RectangleGeometry { Rect = new System.Windows.Rect(50, 50, 50, 50) },
					})
				}
			}
		};

		protected override Brush Convert(string value, object parameter)
		{
			value = value ?? "";
			if (value.ToLower() == AppSettings.TAG_MARKDOWN || value.ToLower() == AppSettings.TAG_LIST)
			{
				return HighlightBrush;
			}
			else
			{
				return Brushes.LightGray;
			}
		}
	}
}
