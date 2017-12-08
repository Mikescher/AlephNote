using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AlephNote.WPF.Controls
{
	/// <summary>
	/// from http://stackoverflow.com/questions/660554/
	/// </summary>
	public class ClickSelectTextBox : TextBox
	{
		public event RoutedEventHandler CtrlExecute;

		public ClickSelectTextBox()
		{
			AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(SelectivelyIgnoreMouseButton), true);
			AddHandler(GotKeyboardFocusEvent, new RoutedEventHandler(SelectAllText), true);
			AddHandler(MouseDoubleClickEvent, new RoutedEventHandler(SelectAllText), true);
			AddHandler(KeyDownEvent, new RoutedEventHandler(KeyDownHandler), true);
		}

		private static void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
		{
			// Find the TextBox
			DependencyObject parent = e.OriginalSource as UIElement;
			while (parent != null && !(parent is TextBox))
				parent = VisualTreeHelper.GetParent(parent);

			if (parent != null)
			{
				var textBox = (TextBox)parent;
				if (!textBox.IsKeyboardFocusWithin)
				{
					// If the text box is not yet focussed, give it the focus and
					// stop further processing of this click event.
					textBox.Focus();
					e.Handled = true;
				}
			}
		}

		private static void SelectAllText(object sender, RoutedEventArgs e)
		{
			if (e.OriginalSource is TextBox tb) tb.SelectAll();
		}

		private void KeyDownHandler(object sender, RoutedEventArgs e)
		{
			var args = (KeyEventArgs)e;

			var valueBinding = GetBindingExpression(TextProperty);
			valueBinding?.UpdateSource();

			if (args.Key == Key.Enter && args.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control))
			{
				CtrlExecute?.Invoke(sender, e);
			}

		}
	}
}
