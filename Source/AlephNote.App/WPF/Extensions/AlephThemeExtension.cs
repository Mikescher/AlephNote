
using AlephNote.Common.Themes;
using AlephNote.WPF.Converter;

namespace AlephNote.WPF.Extensions
{
	public static class AlephThemeExtension
	{
		public static System.Drawing.Color ToDCol(this ColorRef cref)
		{
			return System.Drawing.Color.FromArgb(cref.A, cref.R, cref.G, cref.B);
		}

		public static System.Windows.Media.Color ToWCol(this ColorRef cref)
		{
			return System.Windows.Media.Color.FromArgb(cref.A, cref.R, cref.G, cref.B);
		}

		public static System.Windows.Media.Brush ToWBrush(this ColorRef cref)
		{
			return ColorRefToBrush.Convert(cref);
		}

		public static System.Windows.Media.Brush ToWBrush(this BrushRef cref)
		{
			return BrushRefToBrush.Convert(cref);
		}
	}
}
