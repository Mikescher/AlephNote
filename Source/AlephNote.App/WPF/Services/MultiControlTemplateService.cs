using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AlephNote.WPF.Services
{
	public class ConditionalControlTemplateCollection : ObservableCollection<ConditionalControlTemplate> { }

	public static class MultiControlTemplateService
	{
		public static readonly DependencyProperty MultiControlTemplateProperty = DependencyProperty.RegisterAttached(
			"MultiControlTemplateInternal",
			typeof(ConditionalControlTemplateCollection),
			typeof(MultiControlTemplateService),
			new FrameworkPropertyMetadata(null, MultiTemplateChanged));

		public static ConditionalControlTemplateCollection GetMultiControlTemplate(DependencyObject d)
		{
			var v = (ConditionalControlTemplateCollection)d.GetValue(MultiControlTemplateProperty);
			if (v == null)
			{
				v = new ConditionalControlTemplateCollection();
				v.CollectionChanged += (s, e) => MultiTemplateCollectionChanged(d, e);
				d.SetValue(MultiControlTemplateProperty, v);
			}
			return v;
		}

		public static readonly DependencyProperty MultiControlTemplateSelectorProperty = DependencyProperty.RegisterAttached(
			"MultiControlTemplateSelector",
			typeof(object),
			typeof(MultiControlTemplateService),
			new FrameworkPropertyMetadata(null, SelectorChanged));

		public static object GetMultiControlTemplateSelector(DependencyObject d)
		{
			return d.GetValue(MultiControlTemplateSelectorProperty);
		}

		public static void SetMultiControlTemplateSelector(DependencyObject d, object value)
		{
			d.SetValue(MultiControlTemplateSelectorProperty, value);
		}
		
		private static void SelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			UpdateTemplate(d as ContentControl);
		}

		private static void MultiTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			UpdateTemplate(d as ContentControl);
		}

		private static void MultiTemplateCollectionChanged(DependencyObject d, NotifyCollectionChangedEventArgs e)
		{
			UpdateTemplate(d as ContentControl);
		}

		private static void UpdateTemplate(ContentControl c)
		{
			var selector = GetMultiControlTemplateSelector(c);
			var templates = GetMultiControlTemplate(c);

			// ReSharper disable RedundantCheckBeforeAssignment
			if (templates == null)
			{
				if (c.Template != null) c.Template = null;
			}
			else
			{
				var templ = templates.FirstOrDefault(p => Equals(p.Value, selector));

				if (templ == null)
				{
					if (c.Template != null) c.Template = null;
				}
				else
				{
					if (c.Template != templ.Template) c.Template = templ.Template;
				}
			}
			// ReSharper restore RedundantCheckBeforeAssignment
		}
	}
}
