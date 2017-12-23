using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace AlephNote.WPF.Services
{
	[ContentProperty("Template")]
	public class ConditionalControlTemplate : DependencyObject
	{
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
			"Value",
			typeof(object),
			typeof(ConditionalControlTemplate));

		public object Value
		{
			get { return GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		public static readonly DependencyProperty TemplateProperty = DependencyProperty.Register(
			"Template",
			typeof(ControlTemplate),
			typeof(ConditionalControlTemplate));

		public ControlTemplate Template
		{
			get { return (ControlTemplate)GetValue(TemplateProperty); }
			set { SetValue(TemplateProperty, value); }
		}
	}
}
