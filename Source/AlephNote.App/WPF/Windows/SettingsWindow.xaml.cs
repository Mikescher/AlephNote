using AlephNote.WPF.Converter;
using System.Windows;
using System.Windows.Controls;
using AlephNote.Common.Settings;
using AlephNote.Impl;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Input;

namespace AlephNote.WPF.Windows
{
	/// <summary>
	/// Interaction logic for SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow
	{
		private readonly SettingsWindowViewmodel _viewmodel;
		private readonly MainWindowViewmodel _ownerVM;

		private bool? _multiSelectBackup = null;
		private bool? _hexLineNumberBackup = null;

		public SettingsWindow(MainWindowViewmodel owner, AppSettings data)
		{
			if (string.IsNullOrWhiteSpace(data.ListFontFamily))  data.ListFontFamily  = FontNameToFontFamily.StrDefaultValue;
			if (string.IsNullOrWhiteSpace(data.NoteFontFamily))  data.NoteFontFamily  = FontNameToFontFamily.StrDefaultValue;
			if (string.IsNullOrWhiteSpace(data.TitleFontFamily)) data.TitleFontFamily = FontNameToFontFamily.StrDefaultValue;

			InitializeComponent();

			// HACK: Set this binding in code - otherwise you can't see the underlying panel in the XAML designer
			AdvancedWarningGrid.SetBinding(UIElement.VisibilityProperty, nameof(SettingsWindowViewmodel.HideAdvancedVisibility));

			_ownerVM = owner;
			_viewmodel = new SettingsWindowViewmodel(owner.Owner, data.Clone());
			DataContext = _viewmodel;
		}

		private void OnOKClicked(object sender, RoutedEventArgs e)
		{
			Close();

			_viewmodel.OnBeforeApply();
			if (!_viewmodel.Settings.IsEqual(_ownerVM.Settings)) _ownerVM.ChangeSettings(_viewmodel.Settings);
		}

		private void OnCancelClicked(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void DialogWindow_Closed(object sender, System.EventArgs e)
		{
			_viewmodel.OnBeforeClose();
		}

		private void OnAddAccountClicked(object sender, RoutedEventArgs e)
		{
			Button btn = sender as Button;
			if (btn == null) return;

			btn.ContextMenu = new ContextMenu();

			foreach (var p in PluginManager.Inst.LoadedPlugins)
			{
				var i = new MenuItem() { Header = p.DisplayTitleShort };
				i.Click += (sdr, rea) => _viewmodel.AddAccount(p);
				btn.ContextMenu.Items.Add(i);
			}
			btn.ContextMenu.IsOpen = true;
		}

		private void OnRemAccountClicked(object sender, RoutedEventArgs e)
		{
			_viewmodel.RemoveAccount();
		}

		private void Button_OpenThemeFolder_Click(object sender, RoutedEventArgs e)
		{
			Process.Start("explorer.exe", '"'+App.Themes.BasePath+'"');
		}

		private void Button_Advanced_Click(object sender, RoutedEventArgs e)
		{
			_viewmodel.HideAdvancedVisibility = Visibility.Hidden;
		}

		private void AddSnippet_Click(object sender, RoutedEventArgs e)
		{
			_viewmodel.AddSnippet();
		}

		private void RemoveSnippet_Click(object sender, RoutedEventArgs e)
		{
			_viewmodel.RemoveSnippet();
		}

		private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				if (e.Source is UIElement elt) BindingOperations.GetBindingExpression(elt, TextBox.TextProperty)?.UpdateSource();

				_viewmodel.SelectedSnippet?.Update();
			}
		}
		
		private void RectSelection_Unchecked(object sender, RoutedEventArgs e)
		{
			// Save
			_multiSelectBackup = _viewmodel.Settings.SciMultiSelection;
			_viewmodel.Settings.SciMultiSelection = false;
		}
		
		private void RectSelection_Checked(object sender, RoutedEventArgs e)
		{
			// Restore
			if (_multiSelectBackup != null) _viewmodel.Settings.SciMultiSelection = _multiSelectBackup.Value;
			_multiSelectBackup = null;
		}
		
		private void LineNumbers_Unchecked(object sender, RoutedEventArgs e)
		{
			// Save
			_hexLineNumberBackup = _viewmodel.Settings.SciHexLineNumber;
			_viewmodel.Settings.SciHexLineNumber = false;
		}
		
		private void LineNumbers_Checked(object sender, RoutedEventArgs e)
		{
			// Restore
			if (_hexLineNumberBackup != null) _viewmodel.Settings.SciHexLineNumber = _hexLineNumberBackup.Value;
			_hexLineNumberBackup = null;
		}

        private void SmoothScrollNotesView_Checked(object sender, RoutedEventArgs e)
        {
			if (_ownerVM.Settings.SmoothScrollNotesView) return;

			MessageBox.Show("If you have _many_ notes (100+) this can negatively impact performance.\n"+
						    "Especially in combination with a folder hierarchy this setting can introduce a notable delay when switching the current folder.\n"+
						    "Only enable this setting with a moderate amount of notes", 
							"Warning!", 
							MessageBoxButton.OK, 
							MessageBoxImage.Warning);
        }
    }
}
