using AlephNote.WPF.MVVM;
using System.Linq;
using System.Text.RegularExpressions;

namespace AlephNote.WPF.Converter
{
	class GetLine : OneWayConverter<string, string>
	{
		public GetLine() { }

		protected override string Convert(string value, object parameter)
		{
			int index;
			if (!int.TryParse(System.Convert.ToString(parameter), out index)) return "";

			return Regex.Split(value, @"\r?\n").Where(p => !string.IsNullOrWhiteSpace(p)).Skip(index).FirstOrDefault() ?? "";
		}
	}
}
