
using AlephNote.Common.Themes;

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
	}
}
