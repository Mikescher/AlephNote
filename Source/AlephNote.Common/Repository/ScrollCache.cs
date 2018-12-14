using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AlephNote.Common.Util;
using MSHC.Util.Threads;

namespace AlephNote.Common.Repository
{
	public class ScrollCache
	{
		private readonly Dictionary<string, Tuple<int, int?>> _cache;
		private readonly string _filepath;

		private readonly object _masterLock = new object();

		private readonly DelayedCombiningInvoker invSave;

		private ScrollCache(string fp)
		{
			_cache = new Dictionary<string, Tuple<int, int?>>();
			_filepath = fp;

			invSave = DelayedCombiningInvoker.Create(SaveDirect, 15 * 1000, 2 * 60 * 1000);
		}

		public static ScrollCache LoadFromFile(string path)
		{
			try
			{
				var sc = new ScrollCache(path);
				if (!File.Exists(path)) return sc;

				foreach (var line in File.ReadAllLines(path, Encoding.UTF8))
				{
					if (string.IsNullOrWhiteSpace(line)) continue;
					var split = line.Split('\t');

					if (split.Length==2)
						sc._cache.Add(split[0], Tuple.Create(int.Parse(split[1]), (int?)null));
					else
						sc._cache.Add(split[0], Tuple.Create(int.Parse(split[1]), (int?)int.Parse(split[2])));
				}

				return sc;
			}
			catch (Exception e)
			{
				LoggerSingleton.Inst.Error("ScrollCache", $"Could not load ScrollCache from file '{path}'", e);
				return new ScrollCache(path);
			}
		}

		public static ScrollCache CreateEmpty(string path)
		{
			return new ScrollCache(path);
		}

		public Tuple<int, int?> Get(INote n)
		{
			if (n == null) return null;
			lock (_masterLock)
			{
				return _cache.TryGetValue(n.UniqueName, out var pos) ? pos : null;
			}
		}

		public void Set(INote n, int scrollpos, int cursorpos)
		{
			if (n == null) return;
			lock (_masterLock)
			{
				_cache[n.UniqueName] = new Tuple<int, int?>(scrollpos, cursorpos);
			}
			SetDirty();
		}

		public void SetNoSave(INote n, int scrollpos, int cursorpos)
		{
			if (n == null) return;
			lock (_masterLock)
			{
				_cache[n.UniqueName] = new Tuple<int, int?>(scrollpos, cursorpos);
			}
		}

		public void SetDirty()
		{
			invSave.Request();
		}

		public void ForceSaveNow()
		{
			invSave.CancelPendingRequests();
			SaveDirect();
		}

		private void SaveDirect()
		{
			lock(_masterLock)
			{
				try
				{
					StringBuilder b = new StringBuilder();
					foreach (var ci in _cache) b.AppendLine($"{ci.Key}\t{ci.Value.Item1}\t{ci.Value.Item2}");
					File.WriteAllText(_filepath, b.ToString(), Encoding.UTF8);
				}
				catch (Exception e)
				{
					LoggerSingleton.Inst.Error("ScrollCache", $"Could not save ScrollCache to file '{_filepath}'", e);
				}
			}
		}
	}
}
