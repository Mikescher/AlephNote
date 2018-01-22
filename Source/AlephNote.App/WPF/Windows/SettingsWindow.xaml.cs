using AlephNote.WPF.Converter;
using System.Windows;
using System.Windows.Controls;
using AlephNote.Common.Settings;
using AlephNote.Impl;
using System.Diagnostics;

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
			viewmodel.OnBeforeApply();
			if (!viewmodel.Settings.IsEqual(ownerVM.Settings)) ownerVM.ChangeSettings(viewmodel.Settings);
			Close();
		}

		private void OnCancelClicked(object sender, RoutedEventArgs e)
		{
			Close();
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
	}
}
