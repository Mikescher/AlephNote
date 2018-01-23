using AlephNote.Common.Settings;
using AlephNote.Common.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		public AlephTheme CurrentTheme { get; private set; }

		private WeakList<IThemeListener> _listener = new WeakList<IThemeListener>();

		private ThemeManager(ThemeCache c)
		{
			Cache = c;
			CurrentTheme = c.GetDefault();
		}

		public void LoadWithErrorDialog(AppSettings settings)
		{
			var theme = Cache.GetByFilename(settings.Theme, out var cte);
			if (theme == null)
			{
				LoggerSingleton.Inst.ShowExceptionDialog($"Could not load theme {settings.Theme}", cte);
				theme = Cache.GetDefault();
			}
			else if (!theme.Compatibility.Includes(LoggerSingleton.Inst.AppVersion))
			{
				LoggerSingleton.Inst.ShowExceptionDialog($"Could not load theme {settings.Theme}\r\nThe theme does not support the current version of AlephNote ({LoggerSingleton.Inst.AppVersion})", null);
				theme = Cache.GetDefault();
			}

			ChangeTheme(theme);
		}

		public void ChangeTheme(string xmlname)
		{
			ChangeTheme(Cache.GetByFilename(xmlname, out _) ?? Cache.GetDefault());
		}

		public void ChangeTheme(AlephTheme t)
		{
			CurrentTheme = t;

			foreach (var s in _listener) s.UpdateTargetValue();
		}

		public void RegisterSlave(IThemeListener slave)
		{
			_listener.Add(slave);
		}
	}
}
