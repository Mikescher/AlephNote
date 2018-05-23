using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
	/// Interaction logic for DiffViewer.xaml
	/// </summary>
	public partial class DiffViewer : UserControl, INotifyPropertyChanged
	{
		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void OnExplicitPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		public static readonly DependencyProperty TextLeftProperty =
			DependencyProperty.Register(
			"TextLeft",
			typeof(string),
			typeof(DiffViewer),
			new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.None, (o,e) => { ((DiffViewer)o).OnTextChanged(e); }));

		public static readonly DependencyProperty TextRightProperty =
			DependencyProperty.Register(
			"TextRight",
			typeof(string),
			typeof(DiffViewer),
			new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.None, (o,e) => { ((DiffViewer)o).OnTextChanged(e); }));

		public string TextLeft
		{
			get => (string)GetValue(TextLeftProperty);
			set => SetValue(TextLeftProperty, value);
		}

		public string TextRight
		{
			get => (string)GetValue(TextRightProperty);
			set => SetValue(TextRightProperty, value);
		}

		private SideBySideDiffModel _diff;
		public SideBySideDiffModel Diff 
		{ 
			get => _diff;
			private set { _diff = value; OnPropertyChanged(); }
		}

		public DiffViewer()
		{
			InitializeComponent();
			Compare();
			
			DiffRoot.DataContext = this;
		}

		private void OnTextChanged(DependencyPropertyChangedEventArgs e)
		{
			Compare();
		}
		
		private void Compare()
		{
			var diffBuilder = new SideBySideDiffBuilder(new Differ());
			Diff = diffBuilder.BuildDiffModel(TextLeft ?? string.Empty, TextRight ?? string.Empty);
		}

		private void ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			leftDiff.ScrollToVerticalOffset(e.VerticalOffset);
			leftDiff.ScrollToHorizontalOffset(e.HorizontalOffset);
			
			rightDiff.ScrollToVerticalOffset(e.VerticalOffset);
			rightDiff.ScrollToHorizontalOffset(e.HorizontalOffset);
		}
	}
}
