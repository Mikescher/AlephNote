using AlephNote.Common.Settings;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Markup;

namespace AlephNote.WPF.MarkupExtensions
{
	/// <summary>
	/// http://stackoverflow.com/q/20911104/1761622
	/// </summary>
	public class EnumSourceExtension : MarkupExtension
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

		private readonly Type _enumType;

		public EnumSourceExtension(Type type)
		{
			_enumType = type;
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			IProvideValueTarget target = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
			if (target != null && target.TargetObject is ComboBox)
			{
				((ComboBox)target.TargetObject).SelectedValuePath = "Value";
			}

			var enumValues = Enum.GetValues(_enumType);
			return enumValues
				.Cast<object>()
				.Where(GetVisible)
				.Select(enumValue => new EnumMember{ Value = enumValue, Display = GetDescription(enumValue) })
				.ToArray();
		}

		private string GetDescription(object enumValue)
		{
			var descriptionAttribute1 = _enumType
										.GetField(enumValue.ToString())
										.GetCustomAttributes(typeof(DescriptionAttribute), false)
										.FirstOrDefault() as DescriptionAttribute;

			if (descriptionAttribute1 != null) return descriptionAttribute1.Description;

			var descriptionAttribute2 = _enumType
										.GetField(enumValue.ToString())
										.GetCustomAttributes(typeof(EnumDescriptorAttribute), false)
										.FirstOrDefault() as EnumDescriptorAttribute;

			if (descriptionAttribute2 != null) return descriptionAttribute2.Description;

			return enumValue.ToString();
		}

		private bool GetVisible(object enumValue)
		{
			var descriptionAttribute2 = _enumType
										.GetField(enumValue.ToString())
										.GetCustomAttributes(typeof(EnumDescriptorAttribute), false)
										.FirstOrDefault() as EnumDescriptorAttribute;

			if (descriptionAttribute2 != null) return descriptionAttribute2.Visible;

			return true;
		}

	}
}
