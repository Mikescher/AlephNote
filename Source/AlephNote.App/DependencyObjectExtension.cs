using System.Windows;
using System.Windows.Media;

namespace AlephNote
{
	// https://github.com/JonghoL/EventBindingMarkup/blob/master/src/EventBinding/MVVM/DependencyObjectExtensions.cs
	public static class DependencyObjectExtensions
	{
		public static DependencyObject GetParentObject(this DependencyObject child)
		{
			if (child == null) return null;

			var contentElement = child as ContentElement;
			if (contentElement != null)
			{
				DependencyObject parent = ContentOperations.GetParent(contentElement);
				if (parent != null) return parent;

				FrameworkContentElement fce = contentElement as FrameworkContentElement;
				return fce != null ? fce.Parent : null;
			}

			var frameworkElement = child as FrameworkElement;
			if (frameworkElement != null)
			{
				DependencyObject parent = frameworkElement.Parent;
				if (parent != null) return parent;
			}

			return VisualTreeHelper.GetParent(child);
		}
	}
}
