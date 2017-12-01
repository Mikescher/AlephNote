using System;
using System.Collections.Generic;
using System.Linq;
using AlephNote.WPF.Windows;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using AlephNote.Common.Settings.Types;
using System.Windows.Input;

namespace AlephNote.WPF.Shortcuts
{
	public class GlobalShortcutManager
	{
		private const int HOTKEY_BASE_ID = 9000;

		[DllImport("user32.dll")]
		private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
		[DllImport("user32.dll")]
		private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

		private readonly MainWindow _window;

		private IntPtr _windowHandle;
		private HwndSource _source;

		private bool init = false;

		// <modifiers, key, hotkey_id, action_ident>
		private List<Tuple<uint, uint, int, string>> _hotkeys = new List<Tuple<uint, uint, int, string>>();
		private int _nextID = HOTKEY_BASE_ID;

		public GlobalShortcutManager(MainWindow win)
		{
			_window = win;
		}

		public void Register(AlephModifierKeys mod, AlephKey key, string action)
		{
			var imod = (uint)mod;
			var ikey = (uint)KeyInterop.VirtualKeyFromKey((Key)key);

			UnregisterHotKey(mod, key, action);

			if (init) RegisterHotKey(_windowHandle, _nextID, imod, ikey);

			_hotkeys.Add(Tuple.Create(imod, ikey, _nextID, action));
			_nextID++;
		}

		public void UnregisterHotKey(AlephModifierKeys mod, AlephKey key, string action)
		{
			var imod = (uint)mod;
			var ikey = (uint)KeyInterop.VirtualKeyFromKey((Key)key);

			for (;;)
			{
				var old = _hotkeys.FirstOrDefault(hk => hk.Item1 == imod && hk.Item2 == ikey);
				if (old == null) return;
				_hotkeys.Remove(old);

				if (init) UnregisterHotKey(_windowHandle, old.Item3);
			}
		}

		public void OnSourceInitialized()
		{
			_windowHandle = new WindowInteropHelper(_window).Handle;
			_source = HwndSource.FromHwnd(_windowHandle);
			_source.AddHook(HwndHook);

			foreach (var hk in _hotkeys)
			{
				RegisterHotKey(_windowHandle, hk.Item3, hk.Item1, hk.Item2);
			}

			init = true;
		}

		private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			const int WM_HOTKEY = 0x0312;
			switch (msg)
			{
				case WM_HOTKEY:

					var hkid = wParam.ToInt32();

					var hk = _hotkeys.FirstOrDefault(p => p.Item3 == hkid && p.Item2 == (((int)lParam >> 16) & 0xFFFF));
					if (hk != null)
					{
						ShortcutManager.Execute(_window, hk.Item4);
						handled = true;
					}
					break;
			}
			return IntPtr.Zero;
		}

		public void Close()
		{
			_source.RemoveHook(HwndHook);
			foreach (var hk in _hotkeys) UnregisterHotKey(_windowHandle, hk.Item3);

			init = false;
		}

		public void Clear()
		{
			foreach (var hk in _hotkeys) UnregisterHotKey(_windowHandle, hk.Item3);
		}
	}
}
