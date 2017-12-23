using System.Collections.Generic;
using System.Linq;

namespace AlephNote.PluginInterface
{
	public class DynamicSettingValue
	{
		public enum SettingType { Text, Password, Hyperlink, Checkbox, ComboBox, Folder, Spinner }

		public readonly int ID;
		public readonly string Description;
		public readonly SettingType Type;
		public readonly string CurrentValue;
		public readonly object[] Arguments;
		public readonly string HelpID;

		private DynamicSettingValue(int id, SettingType type, string description, string value, string helpID, object[] args)
		{
			ID = id;
			Type = type;
			Description = description;
			CurrentValue = value;
			Arguments = args;
			HelpID = helpID;
		}

		public static DynamicSettingValue CreateText(int id, string description, string value, string helpID = null)
		{
			return new DynamicSettingValue(id, SettingType.Text, description, value, helpID, new object[0]);
		}

		public static DynamicSettingValue CreatePassword(int id, string description, string value, string helpID = null)
		{
			return new DynamicSettingValue(id, SettingType.Password, description, value, helpID, new object[0]);
		}

		public static DynamicSettingValue CreateCheckbox(int id, string description, bool value, string helpID = null)
		{
			return new DynamicSettingValue(id, SettingType.Checkbox, description, "", helpID, new object[] { value });
		}

		public static DynamicSettingValue CreateHyperlink(string text, string link, string helpID = null)
		{
			return new DynamicSettingValue(-1, SettingType.Hyperlink, text, string.Empty, helpID, new object[] { link });
		}

		public static DynamicSettingValue CreateCombobox(int id, string description, string value, IEnumerable<string> options, string helpID = null)
		{
			return new DynamicSettingValue(id, SettingType.ComboBox, description, value, helpID, options.Cast<object>().ToArray());
		}

		public static DynamicSettingValue CreateFolderChooser(int id, string description, string value, string helpID = null)
		{
			return new DynamicSettingValue(id, SettingType.Folder, description, value, helpID, new object[0]);
		}

		public static DynamicSettingValue CreateNumberChooser(int id, string description, int value, int min, int max, string helpID = null)
		{
			return new DynamicSettingValue(id, SettingType.Spinner, description, "", helpID, new object[] { value, min, max });
		}
	}
}
