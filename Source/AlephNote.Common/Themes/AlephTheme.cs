using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AlephNote.Common.Themes
{
	[DebuggerDisplay("{Name} v{Version} ({SourceFilename})")]
	public sealed class AlephTheme
	{
		public readonly bool IsFallback;

		public string Name { get; set; }
		public Version Version { get; set; }
		public CompatibilityVersionRange Compatibility { get; set; }
		public string SourceFilename { get; set; }

		private Dictionary<string, object> AllProperties = new Dictionary<string, object>();

		public AlephTheme(string n, Version v, CompatibilityVersionRange c, string fn, bool fb)
		{
			Name = n;
			Version = v;
			Compatibility = c;
			SourceFilename = fn;

			IsFallback = fb;
		}

		public void AddProperty(string name, ColorRef prop) => AllProperties.Add(name, prop);
		public void AddProperty(string name, BrushRef prop) => AllProperties.Add(name, prop);
		public void AddProperty(string name, int prop)      => AllProperties.Add(name, prop);
		public void AddProperty(string name, double prop)   => AllProperties.Add(name, prop);
		public void AddProperty(string name, bool prop)     => AllProperties.Add(name, prop);

		public T Get<T>(string name)
		{
			if (IsFallback) return default(T);
			var obj = Get(name);
			if (obj is T result) return result;
			throw new Exception($"ThemeProperty has wrong type: {name} (Expected: {typeof(T)}, Actual: {obj?.GetType()})");
		}

		public object Get(string name)
		{
			if (IsFallback) return null;
			if (AllProperties.TryGetValue(name, out var obj)) return obj;
			throw new Exception($"ThemeProperty not found: {name}");
		}
	}
}
