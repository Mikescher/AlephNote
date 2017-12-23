using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AlephNote.PluginInterface.Util
{
	public static class CollectionExtension
	{
		/// <summary>
		/// Both lists have the same elements after this (but perhaps other order)
		/// </summary>
		public static void Synchronize<T>(this ICollection<T> target, IEnumerable<T> esource)
		{
			var source = esource.ToList();

			foreach (var v in target.Except(source).ToList())
			{
				target.Remove(v);
			}

			foreach (var v in source.Except(target).ToList())
			{
				target.Add(v);
			}
		}

		public static void Synchronize<T>(this ICollection<T> target, IEnumerable<T> esource, out bool changed)
		{
			var source = esource.ToList();

			changed = false;

			foreach (var v in target.Except(source).ToList())
			{
				target.Remove(v);
				changed = true;
			}

			foreach (var v in source.Except(target).ToList())
			{
				target.Add(v);
				changed = true;
			}
		}

		/// <summary>
		/// Both lists have the same elements after this (+ same order)
		/// </summary>
		public static void SynchronizeGenericCollection(this IList target, IEnumerable esource)
		{
			var source = esource.OfType<object>().ToList();

			for (int i = 0; i < Math.Max(target.Count, source.Count);)
			{
				if (i >= source.Count)
				{
					target.RemoveAt(i);
					// not i++
				}
				else if (i >= target.Count)
				{
					target.Insert(i, source[i]);
					i++;
				}
				else if (source[i] == target[i])
				{
					i++;
				}
				else
				{
					target.Insert(i, source[i]);
					i++;
				}
			}
		}

		/// <summary>
		/// Both lists have the same elements after this (+ same order)
		/// </summary>
		public static void SynchronizeCollection<T>(this IList<T> target, IEnumerable<T> esource)
		{
			var source = esource.ToList();

			for (int i = 0; i < Math.Max(target.Count, source.Count);)
			{
				if (i >= source.Count)
				{
					target.RemoveAt(i);
					// not i++
				}
				else if (i >= target.Count)
				{
					target.Insert(i, source[i]);
					i++;
				}
				else if (EqualityComparer<T>.Default.Equals(source[i], target[i]))
				{
					i++;
				}
				else
				{
					target.Insert(i, source[i]);
					i++;
				}
			}
		}

		/// <summary>
		/// Both lists have the same elements after this (+ same order)
		/// </summary>
		public static void SynchronizeCollection<T>(this IList<T> target, IEnumerable<T> esource, Func<T, T, bool> comp, Func<T, T> copy)
		{
			var source = esource.ToList();

			for (int i = 0; i < Math.Max(target.Count, source.Count);)
			{
				if (i >= source.Count)
				{
					target.RemoveAt(i);
					// not i++
				}
				else if (i >= target.Count)
				{
					target.Insert(i, copy(source[i]));
					i++;
				}
				else if (comp(source[i], target[i]))
				{
					i++;
				}
				else
				{
					target.Insert(i, copy(source[i]));
					i++;
				}
			}
		}
	}
}
