using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AlephNote.WPF.Services
{
	public class ConditionalDataTemplateCollection : ObservableCollection<ConditionalDataTemplate> { }

	public static class MultiDataTemplateService
	{
		public static readonly DependencyProperty MultiDataTemplateProperty = DependencyProperty.RegisterAttached(
			"MultiDataTemplateInternal",
			typeof(ConditionalDataTemplateCollection),
			typeof(MultiDataTemplateService),
			new FrameworkPropertyMetadata(null, MultiTemplateChanged));

		public static ConditionalDataTemplateCollection GetMultiDataTemplate(DependencyObject d)
		{
			var v = (ConditionalDataTemplateCollection)d.GetValue(MultiDataTemplateProperty);
			if (v == null)
			{
				v = new ConditionalDataTemplateCollection();
				v.CollectionChanged += (s, e) => MultiTemplateCollectionChanged(d, e);
				d.SetValue(MultiDataTemplateProperty, v);
			}
			return v;
		}

		public static readonly DependencyProperty MultiDataTemplateSelectorProperty = DependencyProperty.RegisterAttached(
			"MultiDataTemplateSelector",
			typeof(object),
			typeof(MultiDataTemplateService),
			new FrameworkPropertyMetadata(null, SelectorChanged));

		public static object GetMultiDataTemplateSelector(DependencyObject d)
		{
			return d.GetValue(MultiDataTemplateSelectorProperty);
		}

		public static void SetMultiDataTemplateSelector(DependencyObject d, object value)
		{
			d.SetValue(MultiDataTemplateSelectorProperty, value);
		}
		
		private static void SelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			UpdateTemplate(d as ItemsControl);
		}

		private static void MultiTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			UpdateTemplate(d as ItemsControl);
		}

		private static void MultiTemplateCollectionChanged(DependencyObject d, NotifyCollectionChangedEventArgs e)
		{
			UpdateTemplate(d as ItemsControl);
		}

		private static void UpdateTemplate(ItemsControl c)
		{
			var selector = GetMultiDataTemplateSelector(c);
			var templates = GetMultiDataTemplate(c);

			// ReSharper disable RedundantCheckBeforeAssignment
			if (templates == null)
			{
				if (c.ItemTemplate != null) c.ItemTemplate = null;
			}
			else
			{
				var templ = templates.FirstOrDefault(p => Equals(p.Value, selector));

				if (templ == null)
				{
					if (c.ItemTemplate != null) c.ItemTemplate = null;
				}
				else
				{
					if (c.ItemTemplate != templ.Template) c.ItemTemplate = templ.Template;
				}
			}
			// ReSharper restore RedundantCheckBeforeAssignment
		}
	}
}
