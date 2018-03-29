using System.Collections;
using System.Collections.Generic;

namespace AlephNote.PluginInterface.Util
{
	public class LimitedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
	{
		public readonly int MaxSize;

		private readonly Dictionary<TKey, TValue> _store;
		private readonly List<TKey> _log;

		public LimitedDictionary(int max)
		{
			MaxSize = max;
			_store = new Dictionary<TKey, TValue>(max+1);
			_log = new List<TKey>(max+1);
		}

		public TValue this[TKey key] 
		{ 
			get => _store[key]; 
			set => SafeAdd(key, value); 
		}

		public ICollection<TKey> Keys => _store.Keys;

		public ICollection<TValue> Values => _store.Values;

		public int Count => _store.Count;

		public bool IsReadOnly => false;

		public void Add(TKey key, TValue value)
		{
			_store.Add(key, value);
			_log.Add(key);
		}

		public void SafeAdd(TKey key, TValue value)
		{
			if (_store.ContainsKey(key))
			{
				_store[key] = value;
				_log.Remove(key);
				_log.Add(key);
			}
			else
			{
				_store.Add(key, value);
				_log.Add(key);
			}

			Cleanup();
		}

		private void Cleanup()
		{
			if (_log.Count>MaxSize) 
			{
				_store.Remove(_log[0]);
				_log.RemoveAt(0);
			}
		}

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			Add(item.Key, item.Value);
		}

		public void Clear()
		{
			_store.Clear();
			_log.Clear();
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return ((IDictionary<TKey, TValue>)_store).Contains(item);
		}

		public bool ContainsKey(TKey key)
		{
			return _store.ContainsKey(key);
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			((IDictionary<TKey, TValue>)_store).CopyTo(array, arrayIndex);
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return ((IDictionary<TKey, TValue>)_store).GetEnumerator();
		}

		public bool Remove(TKey key)
		{
			if (_store.Remove(key))
			{
				_log.Remove(key);
				return true;
			}

			return false;
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			if (((IDictionary<TKey, TValue>)_store).Remove(item))
			{
				_log.Remove(item.Key);
				return true;
			}

			return false;
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			return _store.TryGetValue(key, out value);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _store.GetEnumerator();
		}
	}
}
