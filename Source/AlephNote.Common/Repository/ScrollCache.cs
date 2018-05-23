using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AlephNote.Common.Threading;
using AlephNote.Common.Util;

namespace AlephNote.Common.Repository
{
	public class ScrollCache
	{
		private Dictionary<string, int> _cache;
		private string _filepath;

		private object _masterLock = new object();

		private readonly DelayedCombiningInvoker invSave;

		private ScrollCache(string fp)
		{
			_cache = new Dictionary<string, int>();
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

					sc._cache.Add(split[0], int.Parse(split[1]));
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

		public int? Get(INote n)
		{
			if (n == null) return null;
			lock (_masterLock)
			{
				if (_cache.TryGetValue(n.UniqueName, out int pos))
				{
					return pos;
				}
				return null;
			}
		}

		public void Set(INote n, int pos)
		{
			if (n == null) return;
			lock (_masterLock)
			{
				_cache[n.UniqueName] = pos;
			}
			SetDirty();
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
					foreach (var ci in _cache) b.AppendLine($"{ci.Key}\t{ci.Value}");
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
