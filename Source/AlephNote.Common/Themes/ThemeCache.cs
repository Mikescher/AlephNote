using AlephNote.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using AlephNote.PluginInterface.AppContext;

namespace AlephNote.Common.Themes
{
	public class ThemeCache
	{
		private string _baseThemePath;

		private List<string> _filesInBasePath;         // <path>
		private Dictionary<string, AlephTheme> _cache; // <filename+ext, theme>

		public string BasePath => _baseThemePath;

		public void Init(string basePath)
		{
			_baseThemePath = basePath;
			_cache = new Dictionary<string, AlephTheme>();

			if (!Directory.Exists(_baseThemePath))
			{
				LoggerSingleton.Inst.ShowExceptionDialog($"Themes directory not found: '{_baseThemePath}'", null);
				_filesInBasePath = new List<string>();
				return;
			}

			_filesInBasePath = Directory
				.EnumerateFiles(_baseThemePath)
				.Where(p => p.ToLower().EndsWith(".xml") || p.ToLower().EndsWith(".zip"))
				.ToList();

			if (!_filesInBasePath.Any())
			{
				LoggerSingleton.Inst.ShowExceptionDialog($"No themes found in {_baseThemePath}", null);
			}

			if (_filesInBasePath.All(t => Path.GetFileName(t)?.ToLower() != "default.xml"))
			{
				LoggerSingleton.Inst.ShowExceptionDialog($"No default.xml theme found in {_baseThemePath}", null);
			}
		}
		
		public AlephTheme GetThemeByFilename(string fn, out Exception ex)
		{
			var t = GetByFilename(fn, out ex);
			if (t != null && (t.ThemeType == AlephThemeType.Theme || t.ThemeType == AlephThemeType.Default || t.ThemeType == AlephThemeType.Fallback)) return t;
			return null;
		}
		
		public AlephTheme GetModifierByFilename(string fn, out Exception ex)
		{
			var t = GetByFilename(fn, out ex);
			if (t != null && t.ThemeType == AlephThemeType.Modifier) return t;
			return null;
		}

		private AlephTheme GetByFilename(string fn, out Exception ex)
		{
			ex = null;

			try
			{
				if (_cache.TryGetValue(fn.ToLower(), out var cachedtheme)) return cachedtheme;

				var file = _filesInBasePath.FirstOrDefault(p => Path.GetFileName(p)?.ToLower() == fn.ToLower());
				if (file == null) return null;

				var atheme = LoadFromFile(file);

				_cache.Add(fn.ToLower(), atheme);
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
			return _filesInBasePath
				.Select(Path.GetFileName)
				.Select(p => GetByFilename(p, out _))
				.Concat(_cache.Values)
				.Distinct()
				.Where(p => p != null)
				.Where(p => p.Compatibility.Includes(AlephAppContext.AppVersion))
				.Where(p => p.ThemeType == AlephThemeType.Theme || p.ThemeType == AlephThemeType.Default)
				.ToList();
		}

		public List<AlephTheme> GetAllAvailableThemes()
		{
			return _filesInBasePath
				.Select(Path.GetFileName)
				.Select(p => GetByFilename(p, out _))
				.Concat(_cache.Values)
				.Distinct()
				.Where(p => p != null)
				.Where(p => p.Compatibility.Includes(AlephAppContext.AppVersion))
				.Where(p => p.ThemeType == AlephThemeType.Theme || p.ThemeType == AlephThemeType.Default)
				.ToList();
		}
		
		public List<AlephTheme> GetAllAvailableModifier()
		{
			return _filesInBasePath
				.Select(Path.GetFileName)
				.Select(p => GetByFilename(p, out _))
				.Concat(_cache.Values)
				.Distinct()
				.Where(p => p != null)
				.Where(p => p.Compatibility.Includes(AlephAppContext.AppVersion))
				.Where(p => p.ThemeType == AlephThemeType.Modifier)
				.ToList();
		}
		
		public AlephTheme GetFallback()
		{
			return ThemeParser.GetFallback();
		}

		public AlephTheme GetDefault()
		{
			return GetThemeByFilename("default.xml", out _);
		}

		public AlephTheme GetDefaultOrFallback()
		{
			return GetDefault() ?? GetFallback();
		}

		private AlephTheme LoadFromFile(string file)
		{
			if (file.ToLower().EndsWith(".zip"))
			{
				var parser = new ThemeParser();

				using (var zip = new ZipArchive(File.OpenRead(file), ZipArchiveMode.Read))
				{
					using (var metadata = zip.GetEntry("metadata.xml")?.Open())
					{
						if (metadata == null) throw new Exception("'metadata.xml' not found");
						using (var reader = new StreamReader(metadata, Encoding.UTF8))
						{
							parser.LoadFromString(reader.ReadToEnd(), Path.GetFileName(file));
						}
					}

					foreach (var res in zip.Entries.Where(p => p.Name.ToLower().EndsWith(".ico") || p.Name.ToLower().EndsWith(".png")))
					{
						using (var stream = res.Open())
						{
							using (var ms = new MemoryStream())
							{
								stream.CopyTo(ms);
								parser.AddResource(res.Name, ms.ToArray());
							}
						}
					}
				}

				parser.Parse();
				var t = parser.Generate();
			
				LoggerSingleton.Inst.Debug("ThemeCache", $"Loaded [zip] theme {t.Name} v{t.Version} from '{file}'", $"{string.Join("\n", parser.GetProperties().Select(p => $"{p.Key.PadRight(48, ' ')} {p.Value}"))}");

				return t;
			}
			else
			{
				var parser = new ThemeParser();
				parser.Load(file);
				parser.Parse();
				var t = parser.Generate();
			
				LoggerSingleton.Inst.Debug("ThemeCache", $"Loaded [xml] theme {t.Name} v{t.Version} from '{file}'", $"{string.Join("\n", parser.GetProperties().Select(p => $"{p.Key.PadRight(48, ' ')} {p.Value}"))}");

				return t;
			}
		}

		public void ReplaceTheme(AlephTheme theme)
		{
			_cache[theme.SourceFilename.ToLower()] = theme;
		}
	}
}
