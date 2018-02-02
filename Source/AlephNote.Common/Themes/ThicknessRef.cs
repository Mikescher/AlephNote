using System;
using System.Globalization;
using System.Linq;

namespace AlephNote.Common.Themes
{
	public struct ThicknessRef
	{
		private readonly double _left;
		private readonly double _top;
		private readonly double _right;
		private readonly double _bottom;

		public double Left   => _left;
		public double Top    => _top;
		public double Right  => _right;
		public double Bottom => _bottom;

		private ThicknessRef(double l, double t, double r, double b)
		{
			_left   = l;
			_top    = t;
			_right  = r;
			_bottom = b;
		}

		public override string ToString()
		{
			if (_left == _top && _top == _right && _right == _bottom)
				return _left.ToString(CultureInfo.InvariantCulture);
			if (_left == _right && _top == _bottom)
				return _left.ToString(CultureInfo.InvariantCulture) + "," + _right.ToString(CultureInfo.InvariantCulture);

			return _left.ToString(CultureInfo.InvariantCulture)  + "," +
				   _top.ToString(CultureInfo.InvariantCulture)   + "," +
				   _right.ToString(CultureInfo.InvariantCulture) + "," +
				   _bottom.ToString(CultureInfo.InvariantCulture);
		}

		public static ThicknessRef Create(double v1)
		{
			return new ThicknessRef(v1, v1, v1, v1);
		}

		public static ThicknessRef Create(double v1, double v2)
		{
			return new ThicknessRef(v1, v2, v1, v2);
		}

		public static ThicknessRef Create(double v1, double v2, double v3, double v4)
		{
			return new ThicknessRef(v1, v2, v3, v4);
		}

		public static ThicknessRef Parse(string ovalue)
		{
			var nstyle = NumberStyles.Integer | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
			var nfmt   = CultureInfo.InvariantCulture;

			var value = ovalue.ToLower().Split(',').Select(p => p.Trim()).ToList();
			
			if (value.Count == 1)
			{
				if (!double.TryParse(value[0], nstyle, nfmt, out var v1)) throw new Exception("Error in ThicknessRef format: " + value);

				return Create(v1);
			}
			else if (value.Count == 2)
			{
				if (!double.TryParse(value[0], nstyle, nfmt, out var v1)) throw new Exception("Error in ThicknessRef format: " + value);
				if (!double.TryParse(value[1], nstyle, nfmt, out var v2)) throw new Exception("Error in ThicknessRef format: " + value);

				return Create(v1, v2);
			}
			else if (value.Count == 4)
			{
				if (!double.TryParse(value[0], nstyle, nfmt, out var v1)) throw new Exception("Error in ThicknessRef format: " + value);
				if (!double.TryParse(value[1], nstyle, nfmt, out var v2)) throw new Exception("Error in ThicknessRef format: " + value);
				if (!double.TryParse(value[2], nstyle, nfmt, out var v3)) throw new Exception("Error in ThicknessRef format: " + value);
				if (!double.TryParse(value[3], nstyle, nfmt, out var v4)) throw new Exception("Error in ThicknessRef format: " + value);

				return Create(v1, v2, v3, v4);
			}
			else
			{
				throw new Exception("Error in ThicknessRef format: " + value);
			}
		}
	}
}
