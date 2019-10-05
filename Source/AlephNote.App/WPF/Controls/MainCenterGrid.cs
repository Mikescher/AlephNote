using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AlephNote.WPF.Controls
{
	public enum MainCenterGridChildType { Other, NoteList, Splitter, NoteView }

	public class MainCenterGrid : Grid
	{
		public static readonly DependencyProperty OverviewWidthProperty =
			DependencyProperty.Register(
				"OverviewWidth",
				typeof(GridLength),
				typeof(MainCenterGrid),
				new FrameworkPropertyMetadata(new GridLength(0)));

		public GridLength OverviewWidth
		{
			get { return (GridLength)GetValue(OverviewWidthProperty); }
			set { SetValue(OverviewWidthProperty, value); }
		}
		
		public static readonly DependencyProperty VerticalModeProperty =
			DependencyProperty.Register(
				"VerticalMode",
				typeof(bool),
				typeof(MainCenterGrid),
				new FrameworkPropertyMetadata(false, (o, args) => ((MainCenterGrid)o).OnVerticalModeChanged(args)));

		public bool VerticalMode
		{
			get { return (bool)GetValue(VerticalModeProperty); }
			set { SetValue(VerticalModeProperty, value); }
		}

		public static readonly DependencyProperty ChildTypeProperty =  
			DependencyProperty.RegisterAttached(
				"ChildType", 
				typeof(MainCenterGridChildType),
				typeof(MainCenterGrid), 
				new PropertyMetadata(MainCenterGridChildType.Other));
		
		private bool _isCurrentlyVerticalMode;

		public MainCenterGrid() : base()
		{
			SetHorizontalMode();
		}
		
		private void OnVerticalModeChanged(DependencyPropertyChangedEventArgs args)
		{
			var n = (bool)args.NewValue;

			if (n != _isCurrentlyVerticalMode)
			{
				if (n) SetVerticalMode();
				else   SetHorizontalMode();
			}
		}

		public static MainCenterGridChildType GetChildType(DependencyObject obj)
		{
			return (MainCenterGridChildType)obj.GetValue(ChildTypeProperty);
		}

		public static void SetChildType(DependencyObject obj, MainCenterGridChildType value)
		{
			obj.SetValue(ChildTypeProperty, value);
		}

		private void SetHorizontalMode()
		{
			ColumnDefinitions.Clear();
			RowDefinitions.Clear();

			var b1 = new Binding
			{
				Source = this,
				Path = new PropertyPath("OverviewWidth"),
				Mode = BindingMode.TwoWay,
				UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
			};

			var cdef1 = new ColumnDefinition { MinWidth = 50 };

			BindingOperations.SetBinding(cdef1, ColumnDefinition.WidthProperty, b1);

			ColumnDefinitions.Add(cdef1);
			ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
			ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star), MinWidth = 50 });

			RowDefinitions.Add(new RowDefinition{ Height = new GridLength(1, GridUnitType.Star) });
			
			foreach (UIElement child in Children)
			{
				switch (GetChildType(child))
				{
					case MainCenterGridChildType.NoteList:
						Grid.SetColumn(child, 0);
						Grid.SetRow(child, 0);
						break;

					case MainCenterGridChildType.Splitter:
						Grid.SetColumn(child, 1);
						Grid.SetRow(child, 0);
						((GridSplitter)child).HorizontalAlignment = HorizontalAlignment.Stretch;
						((GridSplitter)child).VerticalAlignment   = VerticalAlignment.Stretch;
						((GridSplitter)child).Width   = 3;
						((GridSplitter)child).Height  = double.NaN;
						break;

					case MainCenterGridChildType.NoteView:
						Grid.SetColumn(child, 2);
						Grid.SetRow(child, 0);
						break;
				}
			}

			_isCurrentlyVerticalMode = false;
		}

		private void SetVerticalMode()
		{
			ColumnDefinitions.Clear();
			RowDefinitions.Clear();

			var b1 = new Binding
			{
				Source = this,
				Path = new PropertyPath("OverviewWidth"),
				Mode = BindingMode.TwoWay,
				UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
			};

			var rdef1 = new RowDefinition { MinHeight = 50 };

			BindingOperations.SetBinding(rdef1, RowDefinition.HeightProperty, b1);

			RowDefinitions.Add(rdef1);
			RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star), MinHeight = 50 });

			ColumnDefinitions.Add(new ColumnDefinition{ Width = new GridLength(1, GridUnitType.Star) });

			foreach (UIElement child in Children)
			{
				switch (GetChildType(child))
				{
					case MainCenterGridChildType.NoteList:
						Grid.SetColumn(child, 0);
						Grid.SetRow(child, 0);
						break;

					case MainCenterGridChildType.Splitter:
						Grid.SetColumn(child, 0);
						Grid.SetRow(child, 1);
						((GridSplitter)child).HorizontalAlignment = HorizontalAlignment.Stretch;
						((GridSplitter)child).VerticalAlignment   = VerticalAlignment.Stretch;
						((GridSplitter)child).Width   = double.NaN;
						((GridSplitter)child).Height  = 3;
						break;

					case MainCenterGridChildType.NoteView:
						Grid.SetColumn(child, 0);
						Grid.SetRow(child, 2);
						break;
				}
			}

			_isCurrentlyVerticalMode = true;
		}
	}
}
