using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace AlephNote.WPF.Util
{
	public static class WPFHelper
	{
		public static T GetChildOfType<T>(this DependencyObject depObj) where T : DependencyObject
		{
			if (depObj == null) return null;

			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
			{
				var child = VisualTreeHelper.GetChild(depObj, i);
				if (child is T c) return c;

				var result = GetChildOfType<T>(child);
				if (result != null) return result;
			}
			return null;
		}
		
		public static T GetParentOfType<T>(DependencyObject initial) where T : DependencyObject
		{
			DependencyObject current = initial;
			while (current != null && current.GetType() != typeof(T)) current = VisualTreeHelper.GetParent(current);
			return current as T;
		}

		public static TreeViewItem VisualTVUpwardSearch(DependencyObject source)
		{
			while (source != null && !(source is TreeViewItem))
				source = VisualTreeHelper.GetParent(source);

			return source as TreeViewItem;
		}

		public static ListViewItem VisualLVUpwardSearch(DependencyObject source)
		{
			while (source != null && !(source is ListViewItem))
			{
				if (source is Run)
				{
					source = LogicalTreeHelper.GetParent(source);
					continue;
				}
				source = VisualTreeHelper.GetParent(source);
			}

			return source as ListViewItem;
		}

		public static ScrollViewer GetScrollViewer(DependencyObject o)
		{
			// Return the DependencyObject if it is a ScrollViewer
			if (o is ScrollViewer s) return s;

			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
			{
				var child = VisualTreeHelper.GetChild(o, i);

				var result = GetScrollViewer(child);
				if (result != null) return result;
			}
			return null;
		}

		public static void ExecDelayed(int ms, Action a)
		{
			new Thread(() => 
			{
				Thread.Sleep(ms);
				Application.Current?.Dispatcher.BeginInvoke(a);
			}).Start();

		}
	}
}
