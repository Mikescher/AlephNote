using System.Collections.Generic;
using System.Linq;

namespace AlephNote
{
	public static class CollectionExtension
	{
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

		public static void SynchronizeSequence<T>(this IList<T> target, IEnumerable<T> esource)
		{
			var source = esource.ToList();

			for (int i = 0; i < target.Count;)
			{
				if (i >= source.Count)
				{
					target.RemoveAt(i);
					// not i++
				}
				else
				{
					if (EqualityComparer<T>.Default.Equals(source[i], target[i]))
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
}
