using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace AlephNote.WPF.Controls
{
	public class DialogWindow : Window
	{
		[DllImport("user32.dll")]
		private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
		[DllImport("user32.dll")]
		private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		private const int GWL_STYLE = -16;

		private const int WS_MAXIMIZEBOX = 0x10000;
		private const int WS_MINIMIZEBOX = 0x20000;

		protected DialogWindow()
		{
			SourceInitialized += OnSourceInitialized;
		}

		private void OnSourceInitialized(object sender, EventArgs e)
		{
			var windowHandle = new WindowInteropHelper(this).Handle;

			if (windowHandle == IntPtr.Zero)
				throw new InvalidOperationException("The window has not yet been completely initialized");

			SetWindowLong(windowHandle, GWL_STYLE, GetWindowLong(windowHandle, GWL_STYLE) & ~(WS_MAXIMIZEBOX | WS_MINIMIZEBOX));
		}
	}
}
