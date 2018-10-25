using AlephNote.Common.Settings;
using AlephNote.Common.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlephNote.PluginInterface;
using AlephNote.PluginInterface.AppContext;

namespace AlephNote.Common.Util
{
    public class ThemeManager
	{
		private static readonly object _lock = new object();

		public static ThemeManager Inst { get; private set; }

		public static void Register(ThemeCache c)
		{
			lock (_lock)
			{
				if (Inst != null) throw new NotSupportedException();

				Inst = new ThemeManager(c);
			}
		}

		public readonly ThemeCache Cache;

		public AlephTheme CurrentBaseTheme { get; private set; }
		public IReadOnlyList<AlephTheme> CurrentModifers { get; private set; }

		public AlephThemeSet CurrentThemeSet { get; private set; }

		private readonly List<IThemeListener> _listener = new List<IThemeListener>();

		private ThemeManager(ThemeCache c)
		{
			Cache = c;
			CurrentBaseTheme = c.GetFallback();
		}

		public void LoadWithErrorDialog(AppSettings settings)
		{
			var theme = Cache.GetThemeByFilename(settings.Theme, out var cte);
			if (theme == null)
			{
				LoggerSingleton.Inst.ShowExceptionDialog($"Could not load theme {settings.Theme}", cte);
				theme = Cache.GetFallback();
			}
			else if (!theme.Compatibility.Includes(AlephAppContext.AppVersion))
			{
				LoggerSingleton.Inst.ShowExceptionDialog($"Could not load theme {settings.Theme}\r\nThe theme does not support the current version of AlephNote ({AlephAppContext.AppVersion.Item1}.{AlephAppContext.AppVersion.Item2}.{AlephAppContext.AppVersion.Item3}.{AlephAppContext.AppVersion.Item4})", null);
				theme = Cache.GetFallback();
			}
			List<AlephTheme> modifiers = new List<AlephTheme>();
			foreach (var mod in settings.ThemeModifier)
			{
				var thememod = Cache.GetModifierByFilename(mod, out var cmte);
				if (thememod == null)
				{
					LoggerSingleton.Inst.ShowExceptionDialog($"Could not load theme-modifier {settings.Theme}", cmte);
					continue;
				}
				else if (!thememod.Compatibility.Includes(AlephAppContext.AppVersion))
				{
					LoggerSingleton.Inst.ShowExceptionDialog($"Could not load theme-modifier {settings.Theme}\r\nThe modifier does not support the current version of AlephNote ({AlephAppContext.AppVersion.Item1}.{AlephAppContext.AppVersion.Item2}.{AlephAppContext.AppVersion.Item3}.{AlephAppContext.AppVersion.Item4})", null);
					continue;
				}
				modifiers.Add(thememod);
			}

			LoggerSingleton.Inst.Info("ThemeManager", $"Set application theme to {theme.Name} (modifiers: {string.Join(" + ", modifiers.Select(m => "["+m.Name+"]"))})");

			ChangeTheme(theme, modifiers);
		}

		public void ChangeTheme(string xmlname, IEnumerable<string> modifierxmlnames)
		{
			ChangeTheme(Cache.GetThemeByFilename(xmlname, out _) ?? Cache.GetFallback(), modifierxmlnames.Select(fn => Cache.GetModifierByFilename(fn, out _)).Where(p => p != null));
		}

		public void ChangeTheme(AlephTheme t, IEnumerable<AlephTheme> m)
		{
			CurrentBaseTheme = t;
			CurrentModifers = m.ToList();

			CurrentThemeSet = new AlephThemeSet(Cache.GetDefaultOrFallback(), CurrentBaseTheme, CurrentModifers);

			_listener.RemoveAll(l => !l.IsTargetAlive);
			foreach (var s in _listener.Where(l => l.IsTargetAlive)) s.OnThemeChanged();
		}

		public void RegisterSlave(IThemeListener slave)
		{
			_listener.Add(slave);
		}
	}
}
