using AlephNote.Common.Themes;
using AlephNote.Common.Util;
using AlephNote.WPF.Extensions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Resources;

namespace AlephNote.WPF.Controls
{
	public class ThemedMainMenu : Menu, IThemeListener
	{
		private Style _mmStyle;

		public ThemedMainMenu()
		{
			ThemeManager.Inst?.RegisterSlave(this);
			Loaded += OnLoaded;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			InitStyle();

			ReapplyTheme();
		}

		private void InitStyle()
		{
			ResourceDictionary dict = (ResourceDictionary)Application.LoadComponent(new Uri("/WPF/Dictionaries/MenuStyleDict.xaml", UriKind.Relative));

			_mmStyle = (Style)dict["ThemedMainMenuStyle"];
		}

		public void OnThemeChanged()
		{
			ReapplyTheme();
		}

		public void ReapplyTheme()
		{
			Background = ThemeManager.Inst.CurrentTheme.Get<BrushRef>("window.mainmenu:background").ToWBrush();
			Foreground = ThemeManager.Inst.CurrentTheme.Get<ColorRef>("window.mainmenu:foreground").ToWBrush();
			Style = _mmStyle;

			ReapplyTheme(this);
		}

		public void ReapplyTheme(ItemsControl ctrl)
		{
			foreach (var item in ctrl.Items)
			{
				if (item is MenuItem mi)
				{
					//mi.Background = ThemeManager.Inst.CurrentTheme.Get<BrushRef>("window.mainmenu.item:background").ToWBrush();
					//mi.Foreground = ThemeManager.Inst.CurrentTheme.Get<ColorRef>("window.mainmenu.item:foreground").ToWBrush();

					ReapplyTheme(mi);
				}
				else if (item is Separator sep)
				{
					//sep.Background = ThemeManager.Inst.CurrentTheme.Get<BrushRef>("window.mainmenu.seperator:background").ToWBrush();
				}

			}
		}
	}
}
