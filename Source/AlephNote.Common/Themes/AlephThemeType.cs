using System;

namespace AlephNote.Common.Themes
{
	public enum AlephThemeType
	{
		Fallback,
		Default,
		Theme,
		Modifier
	}

	public static class AlephThemeTypeHelper
	{
		public static AlephThemeType Parse(string v)
		{
			if ("fallback".Equals(v, StringComparison.InvariantCultureIgnoreCase)) return AlephThemeType.Fallback;
			if ("default".Equals(v, StringComparison.InvariantCultureIgnoreCase))  return AlephThemeType.Default;
			if ("theme".Equals(v, StringComparison.InvariantCultureIgnoreCase))    return AlephThemeType.Theme;
			if ("modifier".Equals(v, StringComparison.InvariantCultureIgnoreCase)) return AlephThemeType.Modifier;

			throw new ArgumentException("Invalid value for AlephThemeType : '"+v+"'");
		}
	}
}
