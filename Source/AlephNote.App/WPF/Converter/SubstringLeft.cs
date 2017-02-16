using MSHC.WPF.MVVM;
using System;

namespace AlephNote.WPF.Converter
{
	class SubstringLeft : OneWayConverter<string, string>
	{
		public SubstringLeft() { }

		protected override string Convert(string value, object parameter)
		{
			int size;
			if (!int.TryParse(System.Convert.ToString(parameter), out size)) return "";

			var v = (value ?? "").Replace("\r", "").Replace("\n", " ");

			return v.Substring(0, Math.Min(size, v.Length));
		}
	}
}
