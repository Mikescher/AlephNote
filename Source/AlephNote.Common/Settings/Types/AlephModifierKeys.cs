using System;

namespace AlephNote.Common.Settings.Types
{
	// Same as System.Windows.Input.ModifierKeys
	[Flags]
	public enum AlephModifierKeys
	{
		None    = 0x00,
		Alt     = 0x01,
		Control = 0x02,
		Shift   = 0x04,
		Windows = 0x08,
	}
}