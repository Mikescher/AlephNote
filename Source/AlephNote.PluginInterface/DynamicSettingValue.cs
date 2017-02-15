
namespace AlephNote.PluginInterface
{
	public class DynamicSettingValue
	{
		public enum SettingType { Text, Password, Hyperlink }

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

		public static DynamicSettingValue CreateHyperlink(string text, string link)
		{
			return new DynamicSettingValue(-1, SettingType.Hyperlink, text, string.Empty, link);
		}
	}
}
