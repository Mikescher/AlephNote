using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AlephNote.Common.MVVM
{
	public class NaturalSortComparer : IComparer<string>, IDisposable
	{
		private readonly bool _isAscending;

		private Dictionary<string, string[]> table = new Dictionary<string, string[]>();

		public NaturalSortComparer(bool inAscendingOrder = true)
		{
			_isAscending = inAscendingOrder;
		}

		public void Dispose()
		{
			table.Clear();
			table = null;
		}

		int IComparer<string>.Compare(string x, string y)
		{
			if (x == y) return 0;
			if (x == null && y != null) return -1;
			if (x != null && y == null) return +1;

			if (!table.TryGetValue(x, out var x1))
			{
				x1 = Regex.Split(x.Replace(" ", ""), "([0-9]+)");
				table.Add(x, x1);
			}

			if (!table.TryGetValue(y, out var y1))
			{
				y1 = Regex.Split(y.Replace(" ", ""), "([0-9]+)");
				table.Add(y, y1);
			}

			int returnVal;

			for (int i = 0; i < x1.Length && i < y1.Length; i++)
			{
				if (x1[i] != y1[i])
				{
					returnVal = PartCompare(x1[i], y1[i]);
					return _isAscending ? returnVal : -returnVal;
				}
			}

			if (y1.Length > x1.Length)
			{
				returnVal = 1;
			}
			else if (x1.Length > y1.Length)
			{ 
				returnVal = -1; 
			}
			else
			{
				returnVal = 0;
			}

			return _isAscending ? returnVal : -returnVal;
		}

		private static int PartCompare(string left, string right)
		{
			if (!int.TryParse(left, out var x)) return String.Compare(left, right, StringComparison.Ordinal);

			if (!int.TryParse(right, out var y)) return String.Compare(left, right, StringComparison.Ordinal);

			return x.CompareTo(y);
		}
	}
}
