using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AlephNote.WPF.Services
{
	public class ConditionalDataTemplateCollection : ObservableCollection<ConditionalDataTemplate> { }

	public static class MultiDatatemplateService
	{
		public static readonly DependencyProperty MultiTemplateProperty = DependencyProperty.RegisterAttached(
			"MultiTemplateInternal",
			typeof(ConditionalDataTemplateCollection),
			typeof(MultiDatatemplateService),
			new FrameworkPropertyMetadata(null, MultiTemplateChanged));

		public static ConditionalDataTemplateCollection GetMultiTemplate(DependencyObject d)
		{
			var v = (ConditionalDataTemplateCollection)d.GetValue(MultiTemplateProperty);
			if (v == null)
			{
				v = new ConditionalDataTemplateCollection();
				v.CollectionChanged += (s, e) => MultiTemplateCollectionChanged(d, e);
				d.SetValue(MultiTemplateProperty, v);
			}
			return v;
		}

		public static readonly DependencyProperty MultiTemplateSelectorProperty = DependencyProperty.RegisterAttached(
			"MultiTemplateSelector",
			typeof(object),
			typeof(MultiDatatemplateService),
			new FrameworkPropertyMetadata(null, SelectorChanged));

		public static object GetMultiTemplateSelector(DependencyObject d)
		{
			return d.GetValue(MultiTemplateSelectorProperty);
		}

		public static void SetMultiTemplateSelector(DependencyObject d, object value)
		{
			d.SetValue(MultiTemplateSelectorProperty, value);
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
			var selector = GetMultiTemplateSelector(c);
			var templates = GetMultiTemplate(c);

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
