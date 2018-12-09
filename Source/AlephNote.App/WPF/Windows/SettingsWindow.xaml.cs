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
		private readonly SettingsWindowViewmodel viewmodel;
		private readonly MainWindowViewmodel ownerVM;

		public SettingsWindow(MainWindowViewmodel owner, AppSettings data)
		{
			if (string.IsNullOrWhiteSpace(data.ListFontFamily))  data.ListFontFamily  = FontNameToFontFamily.StrDefaultValue;
			if (string.IsNullOrWhiteSpace(data.NoteFontFamily))  data.NoteFontFamily  = FontNameToFontFamily.StrDefaultValue;
			if (string.IsNullOrWhiteSpace(data.TitleFontFamily)) data.TitleFontFamily = FontNameToFontFamily.StrDefaultValue;

			InitializeComponent();

			ownerVM = owner;
			viewmodel = new SettingsWindowViewmodel(owner.Owner, data.Clone());
			DataContext = viewmodel;
		}

		private void OnOKClicked(object sender, RoutedEventArgs e)
		{
			Close();

			viewmodel.OnBeforeApply();
			if (!viewmodel.Settings.IsEqual(ownerVM.Settings)) ownerVM.ChangeSettings(viewmodel.Settings);
		}

		private void OnCancelClicked(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void DialogWindow_Closed(object sender, System.EventArgs e)
		{
			viewmodel.OnBeforeClose();
		}

		private void OnAddAccountClicked(object sender, RoutedEventArgs e)
		{
			Button btn = sender as Button;
			if (btn == null) return;

			btn.ContextMenu = new ContextMenu();

			foreach (var p in PluginManager.Inst.LoadedPlugins)
			{
				var i = new MenuItem() { Header = p.DisplayTitleShort };
				i.Click += (sdr, rea) => viewmodel.AddAccount(p);
				btn.ContextMenu.Items.Add(i);
			}
			btn.ContextMenu.IsOpen = true;
		}

		private void OnRemAccountClicked(object sender, RoutedEventArgs e)
		{
			viewmodel.RemoveAccount();
		}

		private void Button_OpenThemeFolder_Click(object sender, RoutedEventArgs e)
		{
			Process.Start("explorer.exe", '"'+App.Themes.BasePath+'"');
		}

		private void Button_Advanced_Click(object sender, RoutedEventArgs e)
		{
			viewmodel.HideAdvancedVisibility = Visibility.Hidden;
		}

		private void AddSnippet_Click(object sender, RoutedEventArgs e)
		{
			viewmodel.AddSnippet();
		}

		private void RemoveSnippet_Click(object sender, RoutedEventArgs e)
		{
			viewmodel.RemoveSnippet();
		}

		private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				if (e.Source is UIElement elt) BindingOperations.GetBindingExpression(elt, TextBox.TextProperty)?.UpdateSource();

				viewmodel.SelectedSnippet?.Update();
			}
		}
	}
}
