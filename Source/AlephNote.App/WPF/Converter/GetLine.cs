using MSHC.WPF.MVVM;
using System.Linq;
using System.Text.RegularExpressions;

namespace AlephNote.WPF.Converter
{
	class GetLine : OneWayConverter<string, string>
	{
		protected override string Convert(string value, object parameter)
		{
			if (!int.TryParse(System.Convert.ToString(parameter), out var index)) return "";

			var line = (Regex.Split(value, @"\r?\n").Where(p => !string.IsNullOrWhiteSpace(p)).Skip(index).FirstOrDefault() ?? "");
			if (line.Length > 48) line = line.Substring(0, 48-3)+"...";
			return line;
		}
	}
}
