using AlephNote.PluginInterface;
using AlephNote.Settings;
using MSHC.WPF.Controls;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace AlephNote.WPF.Converter
{
	class CreatePluginSettingsGrid : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values.Length != 2 || values.Any(p => p == DependencyProperty.UnsetValue)) return DependencyProperty.UnsetValue;

			var provider = values[0] as IRemoteProvider;
			var settings = values[1] as AppSettings;

			if (provider == null) return DependencyProperty.UnsetValue;
			if (settings == null) return DependencyProperty.UnsetValue;

			var cfg = settings.PluginSettings[provider.GetUniqueID()];

			var grid = new Grid();

			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

			int row = 0;
			foreach (var prop in cfg.ListProperties())
			{
				var xprop = prop;

				switch (prop.Type)
				{
					case DynamicSettingValue.SettingType.Text:
						var tb = new TextBox {Text = prop.CurrentValue};
						tb.TextChanged += (s, a) => cfg.SetProperty(xprop.ID, tb.Text);
						AddComponent(prop, ref row, grid, tb);
						break;

					case DynamicSettingValue.SettingType.Password:
						var pb = new BindablePasswordBox {Password = prop.CurrentValue};
						pb.PasswordChanged += (s, a) => cfg.SetProperty(xprop.ID, pb.Password);
						AddComponent(prop, ref row, grid, pb);
						break;

					case DynamicSettingValue.SettingType.Hyperlink:
						var hl = new TextBlock
						{
							Text = prop.Description,
							TextDecorations = TextDecorations.Underline,
							Foreground = Brushes.Blue,
							Cursor = Cursors.Hand,
							ToolTip = prop.Arguments[0],
						};
						hl.MouseDown += (o, e) => Process.Start((string)xprop.Arguments[0]);
						AddComponent(prop, ref row, grid, hl, false);
						break;

					default:
						return DependencyProperty.UnsetValue;
				}
			}

			return grid;
		}

		private void AddComponent(DynamicSettingValue prop, ref int row, Grid grid, FrameworkElement comp, bool addLabel = true)
		{
			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

			if (addLabel)
			{
				var label = new TextBlock { Text = prop.Description + ":" };
				label.Margin = new Thickness(2);
				label.VerticalAlignment = VerticalAlignment.Center;
				Grid.SetRow(label, row);
				Grid.SetColumn(label, 0);
				grid.Children.Add(label);
			}

			comp.Margin = new Thickness(2);
			comp.VerticalAlignment = VerticalAlignment.Center;

			Grid.SetRow(comp, row);
			Grid.SetColumn(comp, 1);

			grid.Children.Add(comp);

			row++;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
