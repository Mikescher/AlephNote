using AlephNote.Settings;
using AlephNote.WPF.Shortcuts;
using AlephNote.WPF.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AlephNote.WPF.Controls
{
	class AutoActionMenuItem : MenuItem
	{
		public static readonly DependencyProperty AlephActionProperty =
			DependencyProperty.Register(
			"AlephAction",
			typeof(string),
			typeof(AutoActionMenuItem),
			new FrameworkPropertyMetadata(string.Empty, (o,a) => ((AutoActionMenuItem)o).OnActionChanged(a) ));

		public string AlephAction
		{
			get { return (string)GetValue(AlephActionProperty); }
			set { SetValue(AlephActionProperty, value); }
		}

		public static readonly DependencyProperty SettingsProperty =
			DependencyProperty.Register(
			"Settings",
			typeof(AppSettings),
			typeof(AutoActionMenuItem),
			new FrameworkPropertyMetadata(null, (o, a) => ((AutoActionMenuItem)o).OnSettingsChanged(a)));

		public AppSettings Settings
		{
			get { return (AppSettings)GetValue(SettingsProperty); }
			set { SetValue(SettingsProperty, value); }
		}

		public static readonly DependencyProperty ParentAnchorProperty =
			DependencyProperty.Register(
			"ParentAnchor",
			typeof(FrameworkElement),
			typeof(AutoActionMenuItem),
			new FrameworkPropertyMetadata(null, (o, a) => ((AutoActionMenuItem)o).OnAnchorChanged(a)));

		public FrameworkElement ParentAnchor
		{
			get { return (FrameworkElement)GetValue(ParentAnchorProperty); }
			set { SetValue(ParentAnchorProperty, value); }
		}

		public AutoActionMenuItem()
		{
			Click += OnMenuClick;
		}

		private MainWindow GetParent(FrameworkElement o)
		{
			if (o == null) return null;

			var mw = o as MainWindow;
			if (mw != null) return mw;

			var fe = o.Parent as FrameworkElement;
			if (fe == null) return null;

			return GetParent(fe);
		}

		private void Refresh()
		{
			var p = GetParent(ParentAnchor) ?? GetParent(this);
			if (p != null)
				InputGestureText = ShortcutManager.GetGestureStr(p, AlephAction);
			else
				InputGestureText = "?";
		}

		private void OnActionChanged(object a)
		{
			Refresh();
		}

		private void OnSettingsChanged(object a)
		{
			Refresh();
		}

		private void OnAnchorChanged(object a)
		{
			Refresh();
		}

		private void OnMenuClick(object sender, RoutedEventArgs e)
		{
			var p = GetParent(this);
			if (p != null) ShortcutManager.Execute(p, AlephAction);
		}
	}
}
