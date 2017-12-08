using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AlephNote.WPF.Controls
{
	/// <summary>
	/// Interaction logic for PopupHelpButton.xaml
	/// </summary>
	public partial class PHelpBtn
	{
		public static readonly DependencyProperty HelpPropertyProperty =
			DependencyProperty.Register(
			"HelpProperty",
			typeof(string),
			typeof(PHelpBtn),
			new FrameworkPropertyMetadata(""));

		public string HelpProperty
		{
			get => (string)GetValue(HelpPropertyProperty);
			set => SetValue(HelpPropertyProperty, value);
		}

		public PHelpBtn()
		{
			InitializeComponent();
			LayoutRoot.DataContext = this;

			Circle.Fill = Brushes.LightBlue;
		}

		private void Grid_MouseLeave(object sender, MouseEventArgs e)
		{
			if (!Popup.IsOpen) Circle.Fill = Brushes.LightBlue;
		}

		private void Grid_MouseEnter(object sender, MouseEventArgs e)
		{
			if (!Popup.IsOpen) Circle.Fill = Brushes.CornflowerBlue;
		}

		private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			Popup.IsOpen = true;
			Keyboard.Focus(PopupTextBox);
			PopupTextBox.Focus();
		}

		private void Popup_Opened(object sender, EventArgs e)
		{
			Circle.Fill = Brushes.Navy;
		}

		private void Popup_Closed(object sender, EventArgs e)
		{
			Circle.Fill = Brushes.LightBlue;
		}
	}
}
