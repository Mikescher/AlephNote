using MSHC.Lang.Attributes;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace AlephNote.WPF.Util
{
    public class AutoEnumComboBox : ComboBox
	{
		public class EnumMember
		{
			public string Display { get; set; }
			public object Value { get; set; }

			public override string ToString()
			{
				return Display;
			}
		}

		private Type _enumType;

		public AutoEnumComboBox(Type t)
        {
			_enumType = t;

			this.SelectedValuePath = "Value";

			var enumValues = Enum.GetValues(_enumType);
			this.ItemsSource = enumValues
				.Cast<object>()
				.Where(GetVisible)
				.Select(enumValue => new EnumMember { Value = enumValue, Display = GetDescription(enumValue) })
				.ToArray();
		}

		private string GetDescription(object enumValue)
		{
			var descriptionAttribute1 = _enumType
										.GetField(enumValue.ToString())
										.GetCustomAttributes(typeof(DescriptionAttribute), false)
										.OfType<DescriptionAttribute>()
										.FirstOrDefault();

			if (descriptionAttribute1 != null) return descriptionAttribute1.Description;

			var descriptionAttribute2 = _enumType
										.GetField(enumValue.ToString())
										.GetCustomAttributes(typeof(EnumDescriptorAttribute), false)
										.OfType<EnumDescriptorAttribute>()
										.FirstOrDefault();

			if (descriptionAttribute2 != null) return descriptionAttribute2.Description;

			return enumValue.ToString();
		}

		private bool GetVisible(object enumValue)
		{
			var descriptionAttribute2 = _enumType
										.GetField(enumValue.ToString())
										.GetCustomAttributes(typeof(EnumDescriptorAttribute), false)
										.OfType<EnumDescriptorAttribute>()
										.FirstOrDefault();

			if (descriptionAttribute2 != null) return descriptionAttribute2.Visible;

			return true;
		}
	}
}
