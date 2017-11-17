using AlephNote.WPF.MVVM;

namespace AlephNote.WPF.Converter
{
	class TagsToPlaceholder : OneWayConverter<string, string>
	{
		public TagsToPlaceholder() { }

		protected override string Convert(string value, object parameter)
		{
			return (string.IsNullOrWhiteSpace(value)) ? "Add tags..." : string.Empty;
		}
	}
}
