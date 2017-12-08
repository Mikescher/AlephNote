using System;
using System.Collections;
using System.Collections.Generic;

namespace AlephNote.Common.MVVM
{
	/// <summary>
	/// Non-generic class to produce instances of the generic class,
	/// optionally using type inference.
	/// 
	/// @source: http://jonskeet.uk/csharp/miscutil/
	/// </summary>
	public static class ProjectionComparer
	{
		public static ProjectionComparer<TSource, TKey> Create<TSource, TKey>(Func<TSource, TKey> projection, bool invert = false)
		{
			return new ProjectionComparer<TSource, TKey>(projection, invert);
		}

		public static ProjectionComparer<TSource, TKey> Create<TSource, TKey>(TSource ignored, Func<TSource, TKey> projection, bool invert = false)
		{
			return new ProjectionComparer<TSource, TKey>(projection, invert);
		}

	}

	/// <summary>
	/// Class generic in the source only to produce instances of the 
	/// doubly generic class, optionally using type inference.
	/// </summary>
	public static class ProjectionComparer<TSource>
	{
		public static ProjectionComparer<TSource, TKey> Create<TKey>(Func<TSource, TKey> projection, bool invert = false)
		{
			return new ProjectionComparer<TSource, TKey>(projection, invert);
		}
	}

	/// <summary>
	/// Comparer which projects each element of the comparison to a key, and then compares
	/// those keys using the specified (or default) comparer for the key type.
	/// </summary>
	/// <typeparam name="TSource">Type of elements which this comparer will be asked to compare</typeparam>
	/// <typeparam name="TKey">Type of the key projected from the element</typeparam>
	public class ProjectionComparer<TSource, TKey> : IComparer<TSource>, IComparer
	{
		private readonly Func<TSource, TKey> _projection;
		private readonly IComparer<TKey> _comparer;
		private readonly int _inverted;

		public ProjectionComparer(Func<TSource, TKey> projection, bool invert = false)
			: this(projection, null, invert)
		{
		}

		public ProjectionComparer(Func<TSource, TKey> projection, IComparer<TKey> comparer, bool invert = false)
		{
			_comparer = comparer ?? Comparer<TKey>.Default;
			_projection = projection;
			_inverted = invert ? -1 : 1;
		}

		public int Compare(TSource x, TSource y)
		{
			// Don't want to project from nullity
			if (x == null && y == null)
			{
				return 0;
			}
			if (x == null)
			{
				return -1 * _inverted;
			}
			if (y == null)
			{
				return 1 * _inverted;
			}
			return _comparer.Compare(_projection(x), _projection(y)) * _inverted;
		}

		public int Compare(object x, object y)
		{
			return Compare((TSource) x, (TSource) y);
		}
	}
}
