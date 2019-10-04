using System;
using System.Drawing;
using System.Windows.Media.Imaging;
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

		public static System.Windows.Thickness ToWThickness(this ThicknessRef tref)
		{
			return new System.Windows.Thickness(tref.Left, tref.Top, tref.Right, tref.Bottom);
		}

		public static System.Windows.CornerRadius ToCornerRad(this CornerRadiusRef tref)
		{
			return new System.Windows.CornerRadius(tref.Left, tref.Top, tref.Right, tref.Bottom);
		}

		public static BitmapImage GetBitmapImageResource(this AlephThemeSet set, string name)
		{
			return set.GetResource(name, BitmapImageFromByteArray, () => new BitmapImage(AlephTheme.GetDefaultResourceUri(name)));
		}

		public static System.Drawing.Icon GetIconResource(this AlephThemeSet set, string name)
		{
			return set.GetResource(name, IconFromByteArray, () => IconFromResource(AlephTheme.GetDefaultResourceUri(name)));
		}

		public static System.Drawing.Bitmap GetDBitmapResource(this AlephThemeSet set, string name)
		{
			return set.GetResource(name, BitmapFromByteArray, () => BitmapFromResource(AlephTheme.GetDefaultResourceUri(name)));
		}

		private static BitmapImage BitmapImageFromByteArray(byte[] arr)
		{
			using (var ms = new System.IO.MemoryStream(arr))
			{
				var image = new BitmapImage();
				image.BeginInit();
				image.CacheOption = BitmapCacheOption.OnLoad;
				image.StreamSource = ms;
				image.EndInit();
				return image;
			}
		}

		private static System.Drawing.Bitmap BitmapFromByteArray(byte[] arr)
		{
			using (var ms = new System.IO.MemoryStream(arr))
			{
				return new System.Drawing.Bitmap(ms);
			}
		}

		private static System.Drawing.Bitmap BitmapFromResource(Uri uri)
		{
			var sri = System.Windows.Application.GetResourceStream(uri);
			if (sri == null) throw new ArgumentException($"Resource '{uri.AbsoluteUri}' not found");

			using (var ms = sri.Stream)
			{
				return new System.Drawing.Bitmap(ms);
			}
		}

		private static System.Drawing.Icon IconFromResource(Uri uri)
		{
			var sri = System.Windows.Application.GetResourceStream(uri);
			if (sri == null) throw new ArgumentException($"Resource '{uri.AbsoluteUri}' not found");
			return new Icon(sri.Stream);
		}

		private static System.Drawing.Icon IconFromByteArray(byte[] arr)
		{
			using (var ms = new System.IO.MemoryStream(arr))
			{
				return new System.Drawing.Icon(ms);
			}
		}
	}
}
