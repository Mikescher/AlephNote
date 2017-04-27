using System.Windows;
using System.Windows.Markup;

namespace AlephNote.WPF.Services
{
	[ContentProperty("Template")]
	public class ConditionalDataTemplate : DependencyObject
	{
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
			"Value",
			typeof(object),
			typeof(ConditionalDataTemplate));

		public object Value
		{
			get { return GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		public static readonly DependencyProperty TemplateProperty = DependencyProperty.Register(
			"Template",
			typeof(DataTemplate),
			typeof(ConditionalDataTemplate));

		public DataTemplate Template
		{
			get { return (DataTemplate)GetValue(TemplateProperty); }
			set { SetValue(TemplateProperty, value); }
		}
	}
}
