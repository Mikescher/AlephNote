using AlephNote.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AlephNote.Common.Themes
{
	public class ThemeCache
	{
		private string _baseThemePath;

		private List<string> _themeFiles; // <path>

		private Dictionary<string, AlephTheme> _cache; // <path, theme>
		private Dictionary<string, string> _themeDefaultValues;

		public void Init(string basePath)
		{
			_baseThemePath = Path.Combine(basePath, "themes");
			_cache = new Dictionary<string, AlephTheme>();

			if (!Directory.Exists(_baseThemePath))
			{
				LoggerSingleton.Inst.ShowExceptionDialog($"Themes directory not found: '{_baseThemePath}'", null);
				_themeFiles = new List<string>();
				return;
			}

			_themeFiles = Directory.EnumerateFiles(_baseThemePath, "*.xml").ToList();

			if (!_themeFiles.Any())
			{
				LoggerSingleton.Inst.ShowExceptionDialog($"No themes found in {_baseThemePath}", null);
			}

			if (!_themeFiles.Any(t => Path.GetFileName(t).ToLower() == "default.xml"))
			{
				LoggerSingleton.Inst.ShowExceptionDialog($"No default.xml theme found in {_baseThemePath}", null);
			}
		}

		public AlephTheme GetByFilename(string fn, out Exception ex)
		{
			ex = null;

			try
			{
				var file = _themeFiles.FirstOrDefault(p => Path.GetFileName(p).ToLower() == fn.ToLower());

				if (file == null) return null;

				if (_cache.TryGetValue(file, out var cachedtheme)) return cachedtheme;

				var atheme = LoadFromFile(file);

				_cache.Add(file, atheme);
				return atheme;
			}
			catch (Exception e)
			{
				ex = e;
				LoggerSingleton.Inst.Error("ThemeCache", $"Could not load theme from {fn}", e);
				return null;
			}
		}

		public List<AlephTheme> GetAllAvailable()
		{
			return _themeFiles
				.Select(p => Path.GetFileName(p))
				.Select(p => GetByFilename(p, out _))
				.ToList();
		}

		public AlephTheme GetDefault()
		{
			return ThemeParser.GetDefault();
		}

		private AlephTheme LoadFromFile(string file)
		{
			if (Path.GetFileName(file).ToLower() == "default.xml")
			{
				var parser = new ThemeParser();
				parser.Load(file);
				parser.Parse();
				_themeDefaultValues = parser.GetProperties();
				var t = parser.Generate(_themeDefaultValues);

				LoggerSingleton.Inst.Debug("ThemeCache", $"Loaded theme {t.Name} v{t.Version} from '{file}'");

				return t;
			}
			else
			{
				if (_themeDefaultValues == null)
				{
					var dpath = Path.Combine(_baseThemePath, "default.xml");
					var dparser = new ThemeParser();
					dparser.Load(dpath);
					dparser.Parse();
					_themeDefaultValues = dparser.GetProperties();

					LoggerSingleton.Inst.Debug("ThemeCache", $"Loaded {_themeDefaultValues.Count} default values from '{dpath}'");
				}

				var parser = new ThemeParser();
				parser.Load(file);
				parser.Parse();
				_themeDefaultValues = parser.GetProperties();
				var t = parser.Generate(_themeDefaultValues);

				LoggerSingleton.Inst.Debug("ThemeCache", $"Loaded theme {t.Name} v{t.Version} from '{file}'");

				return t;
			}


		}
	}
}
