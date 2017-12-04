using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AlephNote.Common.Repository
{
	public class ScrollCache
	{
		private IAlephLogger _logger;
		private Dictionary<string, int> _cache;
		private string _filepath;

		private object _masterLock = new object();

		private readonly DelayedCombiningInvoker invSave;

		private ScrollCache(string fp, IAlephLogger log)
		{
			_logger = log;
			_cache = new Dictionary<string, int>();
			_filepath = fp;

			invSave = DelayedCombiningInvoker.Create(SaveDirect, 15 * 1000, 2 * 60 * 1000);
		}

		public static ScrollCache LoadFromFile(string path, IAlephLogger log)
		{
			try
			{
				var sc = new ScrollCache(path, log);
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
				log.Error("ScrollCache", $"Could not load ScrollCache from file '{path}'", e);
				return new ScrollCache(path, log);
			}
		}

		public static ScrollCache CreateEmpty(string path, IAlephLogger log)
		{
			return new ScrollCache(path, log);
		}

		public int? Get(INote n)
		{
			if (n == null) return null;
			lock (_masterLock)
			{
				if (_cache.TryGetValue(n.GetUniqueName(), out int pos))
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
				_cache[n.GetUniqueName()] = pos;
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
					_logger.Error("ScrollCache", $"Could not save ScrollCache to file '{_filepath}'", e);
				}
			}
		}
	}
}
