using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AlephNote.WPF.Controls
{
	/// <summary>
	/// Interaction logic for FontBox.xaml
	/// </summary>
	public partial class FontBox
	{
		private static readonly FontFamily defaultValue = new TextBlock().FontFamily;

		public static readonly DependencyProperty SelectedFontProperty =
			DependencyProperty.Register(
			"SelectedFont",
			typeof(FontFamily),
			typeof(FontBox),
			new FrameworkPropertyMetadata(
				defaultValue, 
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public FontFamily SelectedFont
		{
			get { return (FontFamily)GetValue(SelectedFontProperty); }
			set { SetValue(SelectedFontProperty, value); }
		}

		public FontBox()
		{
			InitializeComponent();

			MainCBox.DataContext = this;
		}
	}
}
