using AlephNote.Common.Repository;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using AlephNote.Common.Settings;
using AlephNote.PluginInterface;

namespace AlephNote.WPF.Controls
{
	public partial class ConnectionDisplayControl : INotifyPropertyChanged
	{
		#region AccountChangeEvent

		public delegate void AccountChangeEventHandler(object sender, AccountChangeEventArgs e);
		public class AccountChangeEventArgs : EventArgs
		{
			public readonly Guid AccountID;
			public AccountChangeEventArgs(Guid id) { AccountID = id; }
		}

		#endregion

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

		public static readonly DependencyProperty RepositoryProperty =
			DependencyProperty.Register(
				"Repository",
				typeof(NoteRepository),
				typeof(ConnectionDisplayControl),
				new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, (s, e) => ((ConnectionDisplayControl)s).OnRepositoryChanged()));

		public NoteRepository Repository
		{
			get => (NoteRepository)GetValue(RepositoryProperty);
			set => SetValue(RepositoryProperty, value);
		}

		public static readonly DependencyProperty SettingsProperty =
			DependencyProperty.Register(
				"Settings",
				typeof(AppSettings),
				typeof(ConnectionDisplayControl),
				new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, (s, e) => ((ConnectionDisplayControl)s).OnSettingsChanged()));

		public AppSettings Settings
		{
			get => (AppSettings)GetValue(SettingsProperty);
			set => SetValue(SettingsProperty, value);
		}

		public event AccountChangeEventHandler ChangeAccount;

		public string ConnectionTooltip => $"[Remote Account]\r\nName: {Repository?.ConnectionDisplayTitle}\r\nID: {Repository?.ConnectionUUID}\r\nNotes: {Repository?.Notes?.Count}";

		public ConnectionDisplayControl()
		{
			InitializeComponent();
			LayoutRoot.DataContext = this;
		}

		private void OnRepositoryChanged()
		{
			OnExplicitPropertyChanged("ConnectionTooltip");
		}

		private void OnSettingsChanged()
		{
			OnExplicitPropertyChanged("ConnectionTooltip");
		}

		private void ShowAccountChooser(object sender, MouseButtonEventArgs args)
		{
			if (Settings.Accounts.Count > 1)
			{
				var owner = (FrameworkElement)sender;

				owner.ContextMenu = new ContextMenu();
				foreach (var acc in Settings.Accounts)
				{
					var mi = new MenuItem {Header = acc.DisplayTitle};
					mi.Click += (s, e) => SelectAccount(acc);
					owner.ContextMenu.Items.Add(mi);
				}
				owner.ContextMenu.IsOpen = true;
			}
		}

		private void SelectAccount(RemoteStorageAccount acc)
		{
			ChangeAccount?.Invoke(this, new AccountChangeEventArgs(acc.ID));
		}

		private void UIElement_OnMouseEnter(object sender, MouseEventArgs e)
		{
			if (Settings.Accounts.Count > 1) LayoutRoot.Background = Brushes.LightGray;
			if (Settings.Accounts.Count > 1) OuterBorder.BorderBrush = Brushes.Gray;
		}

		private void UIElement_OnMouseLeave(object sender, MouseEventArgs e)
		{
			if (Settings.Accounts.Count > 1) LayoutRoot.Background = Brushes.Transparent;
			if (Settings.Accounts.Count > 1) OuterBorder.BorderBrush = Brushes.Transparent;
		}
	}
}
