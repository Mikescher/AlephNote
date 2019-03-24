using AlephNote.WPF.Windows;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AlephNote.Common.Settings;
using AlephNote.Common.Shortcuts;

namespace AlephNote.WPF.Controls
{
	class AutoActionMenuItem : MenuItem
	{
		public static readonly DependencyProperty AlephActionProperty =
			DependencyProperty.Register(
			"AlephAction",
			typeof(string),
			typeof(AutoActionMenuItem),
			new FrameworkPropertyMetadata(string.Empty, (o,a) => ((AutoActionMenuItem)o).OnActionChanged()));

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
			new FrameworkPropertyMetadata(null, (o, a) => ((AutoActionMenuItem)o).OnSettingsChanged()));

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
			new FrameworkPropertyMetadata(null, (o, a) => ((AutoActionMenuItem)o).OnAnchorChanged()));

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

			if (o is MainWindow mw) return mw;

			if (o.Parent is FrameworkElement fe) return GetParent(fe);

			return null;
		}

		private void Refresh()
		{
			var p = GetParent(ParentAnchor) ?? GetParent(this) ?? MainWindow.Instance;

			if (p != null)
				InputGestureText = ShortcutManager.GetGestureStr(p, AlephAction);
			else
				InputGestureText = "?";
		}

		private void OnActionChanged()
		{
			Refresh();
		}

		private void OnSettingsChanged()
		{
			Refresh();
		}

		private void OnAnchorChanged()
		{
			Refresh();
		}

		private void OnMenuClick(object sender, RoutedEventArgs e)
		{
			var p = GetParent(ParentAnchor) ?? GetParent(this) ?? MainWindow.Instance;

			if (p != null) ShortcutManager.Execute(p, AlephAction);
		}

		public void RecursiveRefresh()
		{
			Refresh();

			foreach (var aami in Items.OfType<AutoActionMenuItem>())
			{
				aami.RecursiveRefresh();
			}
		}
	}
}
