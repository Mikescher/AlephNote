using MSHC.WPF.MVVM;

namespace AlephNote.WPF.Converter
{
	class StringCoalesce : OneWayConverter<string, string>
	{
		public StringCoalesce() { }

		protected override string Convert(string value, object parameter)
		{
			if (string.IsNullOrWhiteSpace(value)) return parameter.ToString();
			return value;
		}
	}
}
