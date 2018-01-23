using AlephNote.WPF.MVVM;
using System.Windows.Media;
using AlephNote.Common.Settings;
using AlephNote.WPF.Windows;
using AlephNote.Common.Themes;
using AlephNote.Common.Util;

namespace AlephNote.WPF.Converter
{
	class TagToBorderBrush : OneWayConverter<string, Brush>
	{
		private static AlephTheme _currTheme = null;
		private static Brush StandardBrush   = null;
		private static Brush HighlightBrush  = null;

		private static void CreateBrushes(AlephTheme t)
		{
			StandardBrush = ColorRefToBrush.Convert(t.Window_TagEditor_TagBorderNormal);

			HighlightBrush = new DrawingBrush
			{
				Viewport = new System.Windows.Rect(0, 0, 8, 8),
				ViewportUnits = BrushMappingMode.Absolute,
				TileMode = TileMode.Tile,
				Drawing = new GeometryDrawing
				{
					Brush = ColorRefToBrush.Convert(t.Window_TagEditor_TagBorderHighlighted),
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

			StandardBrush.Freeze();
			HighlightBrush.Freeze();

			_currTheme = t;
		}

		protected override Brush Convert(string value, object parameter)
		{
			var theme = ThemeManager.Inst.CurrentTheme;
			if (theme != _currTheme) CreateBrushes(theme);

			value = value ?? "";

			if (value.ToLower() == AppSettings.TAG_MARKDOWN || value.ToLower() == AppSettings.TAG_LIST)
				return HighlightBrush;
			else
				return StandardBrush;
		}
	}
}
