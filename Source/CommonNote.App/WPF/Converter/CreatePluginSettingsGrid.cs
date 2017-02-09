using CommonNote.PluginInterface;
using CommonNote.Settings;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MSHC.WPF.Controls;

namespace CommonNote.WPF.Converter
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
				int propID = prop.ID;

				grid.RowDefinitions.Add(new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)});

				var label = new TextBlock {Text = prop.Description + ":"};

				FrameworkElement comp;
				switch (prop.Type)
				{
					case DynamicSettingValue.SettingType.Text:
						var tb = new TextBox {Text = prop.CurrentValue};
						tb.TextChanged += (s, a) => cfg.SetProperty(propID, tb.Text);
						comp = tb;
						break;
					case DynamicSettingValue.SettingType.Password:
						var pb = new BindablePasswordBox {Password = prop.CurrentValue};
						pb.PasswordChanged += (s, a) => cfg.SetProperty(propID, pb.Password);
						comp = pb;
						break;
					default:
						return DependencyProperty.UnsetValue;
				}

				label.Margin = new Thickness(2);
				comp.Margin = new Thickness(2);

				label.VerticalAlignment = VerticalAlignment.Center;
				comp.VerticalAlignment = VerticalAlignment.Center;

				Grid.SetRow(label, row);
				Grid.SetColumn(label, 0);
				Grid.SetRow(comp, row);
				Grid.SetColumn(comp, 1);

				grid.Children.Add(label);
				grid.Children.Add(comp);

				row++;
			}

			return grid;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
