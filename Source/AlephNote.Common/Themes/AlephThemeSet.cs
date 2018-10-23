using System;
using System.Collections.Generic;
using System.Globalization;

namespace AlephNote.Common.Themes
{
	public class AlephThemeSet
	{
		public readonly AlephTheme DefaultTheme;
		public readonly AlephTheme BaseTheme;
		public readonly IReadOnlyList<AlephTheme> Modifiers;

		public AlephThemeSet(AlephTheme t0, AlephTheme t1, IReadOnlyList<AlephTheme> mm)
		{
			DefaultTheme = t0;
			BaseTheme    = t1;
			Modifiers    = mm;
		}

		public T Get<T>(string name)
		{
			var obj = Get(name);
			if (obj is T result) return result;
			throw new Exception($"ThemeProperty has wrong type: {name} (Expected: {typeof(T)}, Actual: {obj?.GetType()})");
		}

		public object Get(string name)
		{
			return GetResolved(name).DirectValue;
		}

		public AlephThemePropertyValue GetUnresolved(string name)
		{
			for (var i = Modifiers.Count - 1; i >= 0; i--)
			{
				var r1 = Modifiers[i].TryGet(name);
				if (r1 != null) return r1;
			}

			if (BaseTheme != null)
			{
				var r1 = BaseTheme.TryGet(name);
				if (r1 != null) return r1;
			}

			if (DefaultTheme != null)
			{
				var r1 = DefaultTheme.TryGet(name);
				if (r1 != null) return r1;
			}

			throw new Exception($"ThemeProperty not found: {name}");
		}

		public AlephThemePropertyValue GetResolved(string name)
		{
			var original_name = name;

			for (var depth = 0;;depth++)
			{
				if (depth >= 4) throw new Exception($"Max recursion depth reached for property '{original_name}'");

				var r = GetUnresolved(name);
				if (!r.IsIndirect) return r;

				name = r.IndirectionTarget;
			}
		}

		public string GetStrRepr(string name)
		{
			var obj = GetResolved(name);
		
			if (obj == null) return "NULL";
		
			if (obj.DirectValue is double objDouble) return objDouble.ToString(CultureInfo.InvariantCulture);
		
			return obj.DirectValue.ToString();
		}
	}
}
