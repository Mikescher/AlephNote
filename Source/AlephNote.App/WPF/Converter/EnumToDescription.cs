using MSHC.WPF.MVVM;
using System.ComponentModel;
using System.Linq;
using AlephNote.Common.Settings;
using MSHC.Lang.Attributes;

namespace AlephNote.WPF.Converter
{
	class EnumToDescription : OneWayConverter<object, string>
	{
		protected override string Convert(object value, object parameter)
		{
			if (value == null) return "NULL";

			if (value.GetType()
				.GetField(value.ToString())
				.GetCustomAttributes(typeof(DescriptionAttribute), false)
				.FirstOrDefault() is DescriptionAttribute descriptionAttribute1) return descriptionAttribute1.Description;

			if (value.GetType()
				.GetField(value.ToString())
				.GetCustomAttributes(typeof(EnumDescriptorAttribute), false)
				.FirstOrDefault() is EnumDescriptorAttribute descriptionAttribute2) return descriptionAttribute2.Description;

			return value.ToString();
		}
	}
}
