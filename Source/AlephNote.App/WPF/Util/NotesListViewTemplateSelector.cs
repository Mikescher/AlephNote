using AlephNote.WPF.Windows;
using System;
using System.Windows;
using System.Windows.Controls;

namespace AlephNote.WPF.Util
{
	public class NotesListViewTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			FrameworkElement elem = container as FrameworkElement;
			if (elem == null) return null;
			if (item == null) return null;

			if (!(item is MainWindowViewmodel)) throw new ApplicationException();

			if (((MainWindowViewmodel)item).Settings.UseHierachicalNoteStructure)
			{
				return (DataTemplate)elem.FindResource("TemplateHierachical");
			}
			else
			{
				return (DataTemplate)elem.FindResource("TemplateFlat");
			}
		}
	}
}
