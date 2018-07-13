using System.Windows;
using AlephNote.Common.Settings;
using AlephNote.Common.Settings.Types;

namespace AlephNote.WPF.Util
{
	public static class SettingsHelper
	{
		public static void ApplyWindowState(Window w, AppSettings s, bool savePosition = true, bool saveSize = true, bool saveState = true)
		{
			if (w.WindowState == WindowState.Maximized)
			{
				if (savePosition) s.StartupLocation       = ExtendedWindowStartupLocation.CenterScreen;
				if (saveState)    s.StartupState          = ExtendedWindowState.Maximized;
				if (savePosition) s.StartupPositionX      = (int)w.Left;
				if (savePosition) s.StartupPositionY      = (int)w.Top;
				if (saveSize)     s.StartupPositionWidth  = (int)w.Width;
				if (saveSize)     s.StartupPositionHeight = (int)w.Height;
			}
			else if (w.WindowState == WindowState.Minimized)
			{
				if (savePosition) s.StartupLocation       = ExtendedWindowStartupLocation.Manual;
				if (saveState)    s.StartupState          = ExtendedWindowState.Minimized;
				if (savePosition) s.StartupPositionX      = (int)w.Left;
				if (savePosition) s.StartupPositionY      = (int)w.Top;
				if (saveSize)     s.StartupPositionWidth  = (int)w.Width;
				if (saveSize)     s.StartupPositionHeight = (int)w.Height;
			}
			else if (w.WindowState == WindowState.Normal)
			{
				if (savePosition) s.StartupLocation       = ExtendedWindowStartupLocation.Manual;
				if (saveState)    s.StartupState          = ExtendedWindowState.Normal;
				if (savePosition) s.StartupPositionX      = (int)w.Left;
				if (savePosition) s.StartupPositionY      = (int)w.Top;
				if (saveSize)     s.StartupPositionWidth  = (int)w.Width;
				if (saveSize)     s.StartupPositionHeight = (int)w.Height;
			}
		}
	}
}
