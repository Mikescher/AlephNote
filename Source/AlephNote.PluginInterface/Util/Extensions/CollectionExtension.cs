using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace AlephNote.PluginInterface.Util
{
	public static class CollectionExtension
	{
		public static int? FirstOrDefaultIndex<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			int i = 0;
			foreach (var elem in source)
			{
				if (predicate(elem)) return i;

				i++;
			}
			return null;
		}

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

			Debug.Assert(target.CollectionEquals(source));
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

			Debug.Assert(target.CollectionEquals(esource));
		}

		/// <summary>
		/// Both lists have the same elements after this (+ same order)
		/// Uses Move() to prevent removing and inserting same element
		/// </summary>
		public static void SynchronizeCollectionSafe<T>(this ObservableCollection<T> target, IEnumerable<T> esource)
		{
			var source = esource.ToList();

			for (int i = 0; i < source.Count; i++)
			{
				var ins = source[i];

				if (i >= target.Count)
				{
					target.Add(ins);
				}
				else
				{
					if (EqualityComparer<T>.Default.Equals(ins, target[i])) continue;
					
					var match = i + target.Skip(i).FirstOrDefaultIndex(p => EqualityComparer<T>.Default.Equals(ins, p));
					if (match == null)
					{
						target.Insert(i, ins);
					}
					else
					{
						target.Move(match.Value, i);
					}
				}
			}

			while (target.Count > source.Count)
			{
				target.RemoveAt(target.Count-1);
			}

			Debug.Assert(target.CollectionEquals(esource));
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

			Debug.Assert(target.CollectionEquals(esource, comp));
		}

		public static bool CollectionEquals<T>(this IList<T> target, IEnumerable<T> esource)
		{
			var source = esource.ToList();

			if (source.Count != target.Count) return false;
			for (int i = 0; i < source.Count; i++)
			{
				if (!EqualityComparer<T>.Default.Equals(source[i], target[i])) return false;
			}

			return true;
		}

		public static bool CollectionEquals<T>(this IList<T> target, IEnumerable<T> esource, Func<T, T, bool> comp)
		{
			var source = esource.ToList();

			if (source.Count != target.Count) return false;
			for (int i = 0; i < source.Count; i++)
			{
				if (!comp(source[i], target[i])) return false;
			}

			return true;
		}
		
		public static bool CollectionEquals(this IList target, IList source)
		{
			if (source.Count != target.Count) return false;
			for (int i = 0; i < source.Count; i++)
			{
				if (source[i] != target[i]) return false;
			}

			return true;
		}
	}
}
