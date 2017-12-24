using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AlephNote.WPF.Util
{
	public static class WPFHelper
	{
		public static DependencyObject GetChildOfType<T>(this DependencyObject depObj)
		{
			if (depObj == null) return null;

			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
			{
				var child = VisualTreeHelper.GetChild(depObj, i);
				if (child is T) return child;

				var result = GetChildOfType<T>(child);
				if (result != null) return result;
			}
			return null;
		}

		public static TreeViewItem VisualTVUpwardSearch(DependencyObject source)
		{
			while (source != null && !(source is TreeViewItem))
				source = VisualTreeHelper.GetParent(source);

			return source as TreeViewItem;
		}
	}
}
