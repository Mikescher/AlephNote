
using System.Collections.Generic;
using System.Linq;

namespace AlephNote.PluginInterface
{
	public class DynamicSettingValue
	{
		public enum SettingType { Text, Password, Hyperlink, Checkbox, ComboBox, Folder }

		public readonly int ID;
		public readonly string Description;
		public readonly SettingType Type;
		public readonly string CurrentValue;
		public readonly object[] Arguments;

		private DynamicSettingValue(int id, SettingType type, string description, string value, params object[] args)
		{
			ID = id;
			Type = type;
			Description = description;
			CurrentValue = value;
			Arguments = args;
		}

		public static DynamicSettingValue CreateText(int id, string description, string value)
		{
			return new DynamicSettingValue(id, SettingType.Text, description, value);
		}

		public static DynamicSettingValue CreatePassword(int id, string description, string value)
		{
			return new DynamicSettingValue(id, SettingType.Password, description, value);
		}

		public static DynamicSettingValue CreateCheckbox(int id, string description, bool value)
		{
			return new DynamicSettingValue(id, SettingType.Checkbox, description, "", value);
		}

		public static DynamicSettingValue CreateHyperlink(string text, string link)
		{
			return new DynamicSettingValue(-1, SettingType.Hyperlink, text, string.Empty, link);
		}

		public static DynamicSettingValue CreateCombobox(int id, string description, string value, IEnumerable<string> options)
		{
			return new DynamicSettingValue(id, SettingType.ComboBox, description, value, options.Cast<object>().ToArray());
		}

		public static DynamicSettingValue CreateFolderChooser(int id, string description, string value)
		{
			return new DynamicSettingValue(id, SettingType.Folder, description, value);
		}
	}
}
