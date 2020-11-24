using System.Windows.Media;
using AlephNote.Common.Util;
using AlephNote.WPF.Extensions;
using MSHC.WPF.MVVM;
using AlephNote.WPF.Util;
using AlephNote.Common.Hierachy;

namespace AlephNote.WPF.Converter
{
	public class HierachicalWrapperToIcon : OneWayConverter<HierachicalBaseWrapper, ImageSource>
	{
		protected override ImageSource Convert(HierachicalBaseWrapper value, object parameter)
		{
			return ThemeManager.Inst.CurrentThemeSet.GetBitmapImageResource(value.IconSourceKey);
		}
	}
}
