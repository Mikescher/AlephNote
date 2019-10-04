using MSHC.WPF.MVVM;
using System.Windows.Media;
using AlephNote.Common.Util;
using AlephNote.WPF.Extensions;

namespace AlephNote.WPF.Converter
{
	public class ReadonlyToLockIcon : OneWayConverter<bool, ImageSource>
	{
		protected override ImageSource Convert(bool value, object parameter)
		{
			if (value) return ThemeManager.Inst.CurrentThemeSet.GetBitmapImageResource("lock.png");
			else       return ThemeManager.Inst.CurrentThemeSet.GetBitmapImageResource("lock_open.png");
		}
	}
}
