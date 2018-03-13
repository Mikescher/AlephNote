using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AlephNote.WPF.Controls
{
	/// <summary>
	/// Interaction logic for GlobalSearchTextBox.xaml
	/// </summary>
	public partial class GlobalSearchTextBox : UserControl
	{
		public static readonly DependencyProperty SearchTextProperty =
			DependencyProperty.Register(
			"SearchText",
			typeof(string),
			typeof(GlobalSearchTextBox),
			new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public string SearchText
		{
			get { return (string)GetValue(SearchTextProperty); }
			set { SetValue(SearchTextProperty, value); }
		}

		public GlobalSearchTextBox()
		{
			InitializeComponent();
			LayoutRoot.DataContext = this;
		}

		public new void Focus()
		{
			SearchControl.Focus();
			Keyboard.Focus(SearchControl);
		}

		private void Button_Clear_Click(object sender, RoutedEventArgs e)
		{
			SearchText = string.Empty;
		}
	}
}
