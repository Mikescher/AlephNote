using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Point = System.Windows.Point;

namespace AlephNote.WPF
{
	public class WpfScreen
	{
		public static IEnumerable<WpfScreen> AllScreens()
		{
			foreach (Screen screen in Screen.AllScreens)
			{
				yield return new WpfScreen(screen);
			}
		}

		public static WpfScreen GetScreenFrom(Window window)
		{
			WindowInteropHelper windowInteropHelper = new WindowInteropHelper(window);
			Screen screen = Screen.FromHandle(windowInteropHelper.Handle);
			WpfScreen wpfScreen = new WpfScreen(screen);
			return wpfScreen;
		}

		public static WpfScreen GetScreenFrom(Point point)
		{
			int x = (int)Math.Round(point.X);
			int y = (int)Math.Round(point.Y);

			// are x,y device-independent-pixels ??
			System.Drawing.Point drawingPoint = new System.Drawing.Point(x, y);
			Screen screen = Screen.FromPoint(drawingPoint);
			WpfScreen wpfScreen = new WpfScreen(screen);

			return wpfScreen;
		}

		public static WpfScreen Primary
		{
			get { return new WpfScreen(Screen.PrimaryScreen); }
		}

		private readonly Screen screen;

		internal WpfScreen(Screen screen)
		{
			this.screen = screen;
		}

		public Rect DeviceBounds
		{
			get { return GetRect(screen.Bounds); }
		}

		public Rect WorkingArea
		{
			get { return GetRect(screen.WorkingArea); }
		}

		private Rect GetRect(Rectangle value)
		{
			// should x, y, width, height be device-independent-pixels ??
			return new Rect
			{
				X = value.X,
				Y = value.Y,
				Width = value.Width,
				Height = value.Height
			};
		}

		public bool IsPrimary
		{
			get { return screen.Primary; }
		}

		public string DeviceName
		{
			get { return screen.DeviceName; }
		}
	}
}
