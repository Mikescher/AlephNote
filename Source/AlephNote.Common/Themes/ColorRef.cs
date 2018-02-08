using System;
using System.Diagnostics;

namespace AlephNote.Common.Themes
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public struct ColorRef
	{
		public static readonly ColorRef BLACK        = new ColorRef(0xFF_00_00_00);
		public static readonly ColorRef WHITE        = new ColorRef(0xFF_FF_FF_FF);
		public static readonly ColorRef RED          = new ColorRef(0xFF_FF_00_00);
		public static readonly ColorRef GREEN        = new ColorRef(0xFF_00_FF_00);
		public static readonly ColorRef BLUE         = new ColorRef(0xFF_00_00_FF);

		public static readonly ColorRef HALF_GRAY    = new ColorRef(0xFF_80_80_80);
		public static readonly ColorRef QUARTER_GRAY = new ColorRef(0xFF_40_40_40);

		public static readonly ColorRef MAGENTA      = new ColorRef(0xFF_FF_00_FF);
		public static readonly ColorRef TRANSPARENT  = new ColorRef(0x00_00_00_00);

		private const int ARGBAlphaShift = 0x18;
		private const int ARGBRedShift   = 0x10;
		private const int ARGBGreenShift = 0x08;
		private const int ARGBBlueShift  = 0x00;

		private readonly long value;

		public byte R => (byte)((value >> ARGBRedShift)   & 0xFF);
		public byte G => (byte)((value >> ARGBGreenShift) & 0xFF);
		public byte B => (byte)((value >> ARGBBlueShift)  & 0xFF);
		public byte A => (byte)((value >> ARGBAlphaShift) & 0xFF);

		public long ARGB => value;
		public string DebuggerDisplay => $"{R:X2}_{G:X2}_{B:X2} (Alpha={A:000})";

		public bool IsTransparent => (A == 0);

		private ColorRef(long value)
		{
			this.value = value;
		}

		public override string ToString()
		{
			if (A==255) return $"#{R:X2}{G:X2}{B:X2}";
			return $"#{A:X2}{R:X2}{G:X2}{B:X2}";
		}

		public static ColorRef FromArgb(int alpha, int red, int green, int blue)
		{
			return new ColorRef(MakeArgb((byte)alpha, (byte)red, (byte)green, (byte)blue));
		}

		public static ColorRef FromArgb(int red, int green, int blue)
		{
			return new ColorRef(MakeArgb(255, (byte)red, (byte)green, (byte)blue));
		}

		public static ColorRef FromArgb(long argb)
		{
			return new ColorRef(argb & 0xFFFFFFFF);
		}

		public static ColorRef Parse(string ovalue)
		{
			var value = ovalue.ToUpper();
			
			if (value.StartsWith("#")) value = value.Substring(1);

			if (value.Length == 3) // #RGB
			{
				var r = (byte)Convert.ToUInt32(value[0] + "" + value[0], 16);
				var g = (byte)Convert.ToUInt32(value[1] + "" + value[1], 16);
				var b = (byte)Convert.ToUInt32(value[2] + "" + value[2], 16);
				return FromArgb(r, g, b);
			}
			else if (value.Length == 4) // #ARGB
			{
				var a = (byte)Convert.ToUInt32(value[0] + "" + value[0], 16);
				var r = (byte)Convert.ToUInt32(value[1] + "" + value[1], 16);
				var g = (byte)Convert.ToUInt32(value[2] + "" + value[2], 16);
				var b = (byte)Convert.ToUInt32(value[3] + "" + value[3], 16);
				return FromArgb(a, r, g, b);
			}
			else if (value.Length == 6) // #RRGGBB
			{
				var v = Convert.ToInt64(value, 16) | 0xFF_00_00_00;
				return FromArgb(v);
			}
			else if (value.Length == 8) // #AARRGGBB
			{
				var v = Convert.ToInt64(value, 16);
				return FromArgb(v);
			}
			else
			{
				throw new Exception("Unknown ColorRef format: " + value);
			}

		}

		private static long MakeArgb(byte alpha, byte red, byte green, byte blue)
		{
			return (long)(unchecked((uint)(red << ARGBRedShift | green << ARGBGreenShift | blue << ARGBBlueShift | alpha << ARGBAlphaShift))) & 0xFFFFFFFF;
		}

		public static ColorRef GetRandom(Random r)
		{
			return ColorRef.FromArgb(r.Next(255), r.Next(255), r.Next(255));
		}
	}
}
