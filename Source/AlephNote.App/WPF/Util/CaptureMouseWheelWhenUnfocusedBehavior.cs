using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using AlephNote.WPF.Windows;
using ScintillaNET;

namespace AlephNote.WPF.Util
{
	// https://stackoverflow.com/a/14248147/1761622
	public static class CaptureMouseWheelWhenUnfocusedBehavior
	{
		private static readonly HashSet<WindowsFormsHost> TrackedHosts = new HashSet<WindowsFormsHost>();

		private static readonly System.Windows.Forms.IMessageFilter MessageFilter = new MouseWheelMessageFilter();

		private sealed class MouseWheelMessageFilter : System.Windows.Forms.IMessageFilter
		{
			private static System.Drawing.Point LocationFromLParam(IntPtr lParam)
			{
				short x = (short) ((((long) lParam) >> 0) & 0xffff); 
				short y = (short) ((((long) lParam) >> 16) & 0xffff); 
				return new System.Drawing.Point(x, y);
			}

			private static bool ConsiderRedirect(WindowsFormsHost host)
			{
				var control = host.Child;

				return control != null &&
					  !control.IsDisposed &&
					   control.IsHandleCreated &&
					   control.Visible &&
					  !control.Focused;
			}

			private static int DeltaFromWParam(IntPtr wParam)
			{
				return (short)((((long)wParam) >> 16) & 0xffff);
			}

			private static System.Windows.Forms.MouseButtons MouseButtonsFromWParam(IntPtr wParam)
			{
				const int MK_LBUTTON  = 0x0001;
				const int MK_MBUTTON  = 0x0010;
				const int MK_RBUTTON  = 0x0002;
				const int MK_XBUTTON1 = 0x0020;
				const int MK_XBUTTON2 = 0x0040;

				int buttonFlags = (int)((((long)wParam) >> 0) & 0xffff);
				var buttons = System.Windows.Forms.MouseButtons.None;

				if(buttonFlags != 0)
				{
					if((buttonFlags & MK_LBUTTON) == MK_LBUTTON)   buttons |= System.Windows.Forms.MouseButtons.Left;
					if((buttonFlags & MK_MBUTTON) == MK_MBUTTON)   buttons |= System.Windows.Forms.MouseButtons.Middle;
					if((buttonFlags & MK_RBUTTON) == MK_RBUTTON)   buttons |= System.Windows.Forms.MouseButtons.Right;
					if((buttonFlags & MK_XBUTTON1) == MK_XBUTTON1) buttons |= System.Windows.Forms.MouseButtons.XButton1;
					if((buttonFlags & MK_XBUTTON2) == MK_XBUTTON2) buttons |= System.Windows.Forms.MouseButtons.XButton2;
				}

				return buttons;
			}

			public bool PreFilterMessage(ref System.Windows.Forms.Message m)
			{
				const int WM_MOUSEWHEEL = 0x020A;
				if (m.Msg != WM_MOUSEWHEEL) return false;
				
				if (!MainWindow.Instance.IsKeyboardFocusWithin) return false;

				var location = LocationFromLParam(m.LParam);
				foreach(var host in TrackedHosts)
				{
					if(!ConsiderRedirect(host)) continue;

					var p1 = host.PointToScreen(new Point(0, 0));
					var p2 = host.PointToScreen(new Point(host.ActualWidth, host.ActualHeight));

					if(new Rect(p1, p2).Contains(new Point(location.X, location.Y)))
					{
						var delta = DeltaFromWParam(m.WParam);

						{
							// raise event for WPF control
							var mouse = InputManager.Current.PrimaryMouseDevice;
							var args = new MouseWheelEventArgs(mouse, Environment.TickCount, delta) { RoutedEvent = UIElement.MouseWheelEvent };
							host.RaiseEvent(args);
						}

						if (host.Child is Scintilla sci)
						{
							// raise event for Scintilla control
							sci.LineScroll(-Math.Sign(delta)*3, 0);
						}
						else
						{
							// raise event for winforms control
							var buttons = MouseButtonsFromWParam(m.WParam);
							var args = new System.Windows.Forms.MouseEventArgs(buttons, 0, location.X, location.Y, delta);
							var method = typeof(System.Windows.Forms.Control).GetMethod("OnMouseWheel", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
							method?.Invoke(host.Child, new object[] { args });
						}

						return true;

					}
				}
				return false;
			}
		}

		public static bool GetIsEnabled(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsEnabledProperty);
		}

		public static void SetIsEnabled(DependencyObject obj, bool value)
		{
			obj.SetValue(IsEnabledProperty, value);
		}

		public static readonly DependencyProperty IsEnabledProperty =
			DependencyProperty.RegisterAttached(
				"IsEnabled",
				typeof(bool),
				typeof(CaptureMouseWheelWhenUnfocusedBehavior),
				new PropertyMetadata(false, OnIsEnabledChanged));

		private static void OnIsEnabledChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			var wfh = o as WindowsFormsHost;
			if(wfh == null) return;

			if((bool)e.NewValue)
			{
				wfh.Loaded += OnHostLoaded;
				wfh.Unloaded += OnHostUnloaded;
				if(wfh.IsLoaded && TrackedHosts.Add(wfh))
				{
					if(TrackedHosts.Count == 1)
					{
						System.Windows.Forms.Application.AddMessageFilter(MessageFilter);
					}
				}
			}
			else
			{
				wfh.Loaded -= OnHostLoaded;
				wfh.Unloaded -= OnHostUnloaded;
				if(TrackedHosts.Remove(wfh))
				{
					if(TrackedHosts.Count == 0)
					{
						System.Windows.Forms.Application.RemoveMessageFilter(MessageFilter);
					}
				}
			}
		}

		private static void OnHostLoaded(object sender, EventArgs e)
		{
			var wfh = (WindowsFormsHost)sender;
			if(TrackedHosts.Add(wfh))
			{
				if(TrackedHosts.Count == 1)
				{
					System.Windows.Forms.Application.AddMessageFilter(MessageFilter);
				}
			}
		}

		private static void OnHostUnloaded(object sender, EventArgs e)
		{
			var wfh = (WindowsFormsHost)sender;
			if(TrackedHosts.Remove(wfh))
			{
				if(TrackedHosts.Count == 0)
				{
					System.Windows.Forms.Application.RemoveMessageFilter(MessageFilter);
				}
			}
		}
	}
}
