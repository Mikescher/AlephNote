using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AlephNote
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
			var source = esource.OfType<T>().ToList();

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
	}
}
