using AlephNote.Plugins;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AlephNote.WPF.Util
{
	public static class HelpTextsLoader
	{
		private static Dictionary<string, string> _texts = null;

		private static void Load()
		{
			_texts = new Dictionary<string, string>();

			LoadFromResources();
			LoadFromPlugins();
		}

		private static void LoadFromResources()
		{
			var lines = Regex.Split(Properties.Resources.HelpTexts, @"\r?\n");

			string curr = null;
			StringBuilder collector = new StringBuilder();
			for (int i = 0; i < lines.Length; i++)
			{
				string l = lines[i].TrimEnd();
				if (l.StartsWith("[") && l.EndsWith("]"))
				{
					if (curr != null) _texts[curr] = collector.ToString().Trim();
					curr = l.Substring(1, l.Length - 2);
					collector.Clear();
					continue;
				}
				else
				{
					collector.AppendLine(l);
				}
			}
			if (curr != null) _texts[curr] = collector.ToString().Trim();
		}

		private static void LoadFromPlugins()
		{
			foreach (var plugin in PluginManager.Inst.LoadedPlugins)
			{
				foreach (var htxt in plugin.GetHelpTexts())
				{
					_texts.Add(plugin.GetUniqueID().ToString("B") + "::" + htxt.Key, htxt.Value);
				}
			}
		}

		public static string Get(string id)
		{
			if (_texts == null) Load();

			if (_texts.TryGetValue(id, out var v)) return v.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);

			return "%ERROR%";
		}
	}
}
