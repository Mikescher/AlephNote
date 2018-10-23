using AlephNote.WPF.MVVM;
using System.Windows.Media;
using AlephNote.Common.Settings;
using AlephNote.Common.Themes;
using AlephNote.Common.Util;

namespace AlephNote.WPF.Converter
{
	class TagToBorderBrush : OneWayConverter<string, Brush>
	{
		private static AlephThemeSet _currTheme = null;
		private static Brush StandardBrush   = null;
		private static Brush HighlightBrush  = null;

		private static void CreateBrushes(AlephThemeSet t)
		{
			StandardBrush = BrushRefToBrush.Convert(t.Get<BrushRef>("window.tageditor.tag:bordercolor_default"));

			HighlightBrush = new DrawingBrush
			{
				Viewport = new System.Windows.Rect(0, 0, 8, 8),
				ViewportUnits = BrushMappingMode.Absolute,
				TileMode = TileMode.Tile,
				Drawing = new GeometryDrawing
				{
					Brush = BrushRefToBrush.Convert(t.Get<BrushRef>("window.tageditor.tag:bordercolor_highlighted")),
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
			var theme = ThemeManager.Inst.CurrentThemeSet;
			if (theme != _currTheme) CreateBrushes(theme);

			value = value ?? "";

			if (value.ToLower() == AppSettings.TAG_MARKDOWN || value.ToLower() == AppSettings.TAG_LIST)
				return HighlightBrush;
			else
				return StandardBrush;
		}
	}
}
