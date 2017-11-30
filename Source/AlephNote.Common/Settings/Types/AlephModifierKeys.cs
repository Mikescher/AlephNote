using System;

namespace AlephNote.Common.Settings.Types
{
	// Same as System.Windows.Input.ModifierKeys
	[Flags]
	public enum AlephModifierKeys { None = 0, Alt = 1, Control = 2, Shift = 4, Windows = 8 }
}