using System;
using System.Collections.Generic;
using System.Linq;

namespace AlephNote.PluginInterface
{
	public class DynamicSettingValue
	{
		public enum SettingType { Text, Password, Hyperlink, Checkbox, ComboBox, EnumComboBox, Folder, Spinner }

		public readonly int ID;
		public readonly string Description;
		public readonly SettingType Type;
		public readonly string CurrentValue;
		public readonly object[] Arguments;
		public readonly string HelpID;
		public readonly Type ValueType;

		private DynamicSettingValue(int id, SettingType type, string description, string value, string helpID, Type valuetype, object[] args)
		{
			ID           = id;
			Type         = type;
			Description  = description;
			CurrentValue = value;
			Arguments    = args;
			HelpID       = helpID;
			ValueType    = valuetype;
		}

		public static DynamicSettingValue CreateText(int id, string description, string value, string helpID = null)
		{
			return new DynamicSettingValue(id, SettingType.Text, description, value, helpID, typeof(string), new object[0]);
		}

		public static DynamicSettingValue CreatePassword(int id, string description, string value, string helpID = null)
		{
			return new DynamicSettingValue(id, SettingType.Password, description, value, helpID, typeof(string), new object[0]);
		}

		public static DynamicSettingValue CreateCheckbox(int id, string description, bool value, string helpID = null)
		{
			return new DynamicSettingValue(id, SettingType.Checkbox, description, "", helpID, typeof(bool), new object[] { value });
		}

		public static DynamicSettingValue CreateHyperlink(string text, string link, string helpID = null)
		{
			return new DynamicSettingValue(-1, SettingType.Hyperlink, text, string.Empty, helpID, null, new object[] { link });
		}

		public static DynamicSettingValue CreateCombobox(int id, string description, string value, IEnumerable<string> options, string helpID = null)
		{
			return new DynamicSettingValue(id, SettingType.ComboBox, description, value, helpID, typeof(string), options.Cast<object>().ToArray());
		}

		public static DynamicSettingValue CreateEnumCombobox<T>(int id, string description, T value, string helpID = null)
		{
			return new DynamicSettingValue(id, SettingType.EnumComboBox, description, "", helpID, typeof(T), new object[] { value });
		}

		public static DynamicSettingValue CreateFolderChooser(int id, string description, string value, string helpID = null)
		{
			return new DynamicSettingValue(id, SettingType.Folder, description, value, helpID, typeof(string), new object[0]);
		}

		public static DynamicSettingValue CreateNumberChooser(int id, string description, int value, int min, int max, string helpID = null)
		{
			return new DynamicSettingValue(id, SettingType.Spinner, description, "", helpID, typeof(int), new object[] { value, min, max });
		}
	}
}
