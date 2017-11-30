using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace AlephNote.WPF.Util
{
	[ContentProperty("Template")]
	internal class MenuItemContainerTemplateSelector : ItemContainerTemplateSelector
	{
		public DataTemplate Template { get; set; }

		public override DataTemplate SelectTemplate(object item, ItemsControl parentItemsControl)
		{
			return Template;
		}
	}
}
