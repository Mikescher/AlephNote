using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AlephNote.Common.Themes
{
	public enum BrushRefType { Solid, Gradient_Horizontal, Gradient_Vertical }

	public struct BrushRef
	{
		public static readonly BrushRef BLACK        = CreateSolid(ColorRef.FromArgb(0xFF_00_00_00));
		public static readonly BrushRef WHITE        = CreateSolid(ColorRef.FromArgb(0xFF_FF_FF_FF));
		public static readonly BrushRef RED          = CreateSolid(ColorRef.FromArgb(0xFF_FF_00_00));
		public static readonly BrushRef GREEN        = CreateSolid(ColorRef.FromArgb(0xFF_00_FF_00));
		public static readonly BrushRef BLUE         = CreateSolid(ColorRef.FromArgb(0xFF_00_00_FF));

		public static readonly BrushRef HALF_GRAY    = CreateSolid(ColorRef.FromArgb(0xFF_80_80_80));
		public static readonly BrushRef QUARTER_GRAY = CreateSolid(ColorRef.FromArgb(0xFF_40_40_40));

		public static readonly BrushRef MAGENTA      = CreateSolid(ColorRef.FromArgb(0xFF_FF_00_FF));
		public static readonly BrushRef TRANSPARENT  = CreateSolid(ColorRef.FromArgb(0x00_00_00_00));

		private readonly List<Tuple<double, ColorRef>> _steps;
		private readonly BrushRefType _btype;

		public BrushRefType BrushType => _btype;
		public IEnumerable<Tuple<double, ColorRef>> GradientSteps => _steps;

		private BrushRef(BrushRefType t, List<Tuple<double, ColorRef>> steps)
		{
			_btype = t;
			_steps = steps;
		}

		public override string ToString()
		{
			switch (BrushType)
			{
				case BrushRefType.Solid:
					return _steps[0].Item2.ToString();
				case BrushRefType.Gradient_Horizontal:
					return "gradient-h://" + string.Join("|", _steps.Select(s => $"{s.Item1.ToString(CultureInfo.InvariantCulture)}:{s.Item2.ToString()}"));
				case BrushRefType.Gradient_Vertical:
					return "gradient-v://" + string.Join("|", _steps.Select(s => $"{s.Item1.ToString(CultureInfo.InvariantCulture)}:{s.Item2.ToString()}"));
				default:
					return "?";
			}
		}

		public static BrushRef CreateSolid(ColorRef col)
		{
			return new BrushRef(BrushRefType.Solid, new List<Tuple<double, ColorRef>> { Tuple.Create(0.0, col), Tuple.Create(0.0, col) });
		}

		public static BrushRef CreateGradientHorizontal(params Tuple<double, ColorRef>[] steps)
		{
			return new BrushRef(BrushRefType.Gradient_Horizontal, steps.ToList());
		}

		public static BrushRef CreateGradientVertical(params Tuple<double, ColorRef>[] steps)
		{
			return new BrushRef(BrushRefType.Gradient_Vertical, steps.ToList());
		}

		public static BrushRef Parse(string ovalue)
		{
			var value = ovalue.ToLower();
			
			if (value.StartsWith("#"))
			{
				return CreateSolid(ColorRef.Parse(value));
			}
			else if (value.StartsWith("gradient-h://"))
			{
				try
				{
					value = value.Substring("gradient-h://".Length);

					var steps = value
						.Split('|')
						.Select(p => p.Trim())
						.Select(p => Tuple.Create(double.Parse(p.Split(':')[0], CultureInfo.InvariantCulture), ColorRef.Parse(p.Split(':')[1])))
						.ToArray();

					return CreateGradientHorizontal(steps);
				}
				catch (Exception e)
				{
					throw new Exception("Error in BrushRef format: " + value, e);
				}
			}
			else if (value.StartsWith("gradient-v://"))
			{
				try
				{
					value = value.Substring("gradient-v://".Length);

					var steps = value
						.Split('|')
						.Select(p => p.Trim())
						.Select(p => Tuple.Create(double.Parse(p.Split(':')[0], CultureInfo.InvariantCulture), ColorRef.Parse(p.Split(':')[1])))
						.ToArray();

					return CreateGradientVertical(steps);
				}
				catch (Exception e)
				{
					throw new Exception("Error in BrushRef format: " + value, e);
				}
			}
			else if (value.StartsWith("solid://"))
			{
				value = value.Substring("solid://".Length);
				return CreateSolid(ColorRef.Parse(value));
			}
			else
			{
				throw new Exception("Unknown BrushRef format: " + value);
			}
		}
	}
}
