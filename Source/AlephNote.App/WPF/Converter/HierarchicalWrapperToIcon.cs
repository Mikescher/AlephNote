using System.Windows.Media;
using AlephNote.Common.Util;
using AlephNote.WPF.Extensions;
using MSHC.WPF.MVVM;
using AlephNote.WPF.Util;
using AlephNote.Common.Hierarchy;

namespace AlephNote.WPF.Converter
{
	public class HierarchicalWrapperToIcon : OneWayConverter<HierarchicalBaseWrapper, ImageSource>
	{
		protected override ImageSource Convert(HierarchicalBaseWrapper value, object parameter)
		{
			return ThemeManager.Inst.CurrentThemeSet.GetBitmapImageResource(value.IconSourceKey);
		}
	}
}
