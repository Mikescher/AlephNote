using System.Windows;

namespace AlephNote.WPF.Windows
{
	/// <summary>
	/// Interaction logic for DebugTextWindow.xaml
	/// </summary>
	public partial class DebugTextWindow
	{
		private DebugTextWindow()
		{
			InitializeComponent();
		}

		public static void Show(Window owner, string text, string title)
		{
			var dtw = new DebugTextWindow { Owner = owner, Tb1 = {Text = text}, Title = title };

			dtw.ShowDialog();

		}
	}
}
