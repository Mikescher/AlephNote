
namespace CommonNote.PluginInterface
{
	public class DynamicSettingValue
	{
		public enum SettingType { Text, Password }

		public readonly int ID;
		public readonly string Description;
		public readonly SettingType Type;
		public readonly string CurrentValue;

		private DynamicSettingValue(int id, SettingType type, string description, string value)
		{
			ID = id;
			Type = type;
			Description = description;
			CurrentValue = value;
		}

		public static DynamicSettingValue CreateText(int id, string description, string value)
		{
			return new DynamicSettingValue(id, SettingType.Text, description, value);
		}

		public static DynamicSettingValue CreatePassword(int id, string description, string value)
		{
			return new DynamicSettingValue(id, SettingType.Password, description, value);
		}
	}
}
