using System.Collections.Generic;
using System.ComponentModel;

namespace AlephNote.WPF.Extensions
{
	public static class CollectionViewExtension
	{
		public static T FirstOrDefault<T>(this ICollectionView view)
		{
			var enumerator = view.GetEnumerator();
			if (enumerator.MoveNext())
				return (T)enumerator.Current;
			else
				return default(T);
		}

		public static IEnumerable<T> OfType<T>(this ICollectionView view)
		{
			var enumerator = view.GetEnumerator();
			while (enumerator.MoveNext()) if (enumerator.Current is T r) yield return r;
		}
	}
}
