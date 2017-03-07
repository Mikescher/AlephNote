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
				.Select(enumValue => new EnumMember{ Value = enumValue, Display = GetDescription(enumValue) })
				.ToArray();
		}

		private string GetDescription(object enumValue)
		{
			var descriptionAttribute = _enumType
										.GetField(enumValue.ToString())
										.GetCustomAttributes(typeof(DescriptionAttribute), false)
										.FirstOrDefault() as DescriptionAttribute;

			return (descriptionAttribute != null) ? descriptionAttribute.Description : enumValue.ToString();
		}

	}
}
