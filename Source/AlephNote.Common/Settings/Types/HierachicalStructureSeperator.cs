using System;

namespace AlephNote.Common.Settings.Types
{
	public enum HierachicalStructureSeperator
	{
		[EnumDescriptor("Seperated by '/'")]
		SeperatorForwardSlash,

		[EnumDescriptor("Seperated by '\\'")]
		SeperatorRSlash,

		[EnumDescriptor("Seperated by '.'")]
		SeperatorDot,

		[EnumDescriptor("Seperated by '_'")]
		SeperatorUnderscore,

		[EnumDescriptor("Seperated by '%'")]
		SeperatorPercent,

		[EnumDescriptor("Seperated by '%1E%'")]
		SeperatorRecSep,

		[EnumDescriptor("Seperated by '+++'")]
		SeperatorThreePlus,

		[EnumDescriptor("Seperated by '\\t'")]
		SeperatorTab,
	}

	public static class StructureSeperatorHelper
	{
		public static string GetSeperator(HierachicalStructureSeperator hss)
		{
			switch (hss)
			{
				case HierachicalStructureSeperator.SeperatorForwardSlash: return "/";
				case HierachicalStructureSeperator.SeperatorRSlash: return "\\";
				case HierachicalStructureSeperator.SeperatorDot: return ".";
				case HierachicalStructureSeperator.SeperatorUnderscore: return "_";
				case HierachicalStructureSeperator.SeperatorPercent: return "%";
				case HierachicalStructureSeperator.SeperatorRecSep: return "%1E%"; 
				case HierachicalStructureSeperator.SeperatorThreePlus: return "+++";
				case HierachicalStructureSeperator.SeperatorTab: return "\t";
				default:
					throw new ArgumentOutOfRangeException(nameof(hss), hss, null);
			}
		}

		public static char GetEscapeChar(HierachicalStructureSeperator hss)
		{
			switch (hss)
			{
				case HierachicalStructureSeperator.SeperatorForwardSlash: return '\\';
				case HierachicalStructureSeperator.SeperatorRSlash: return '\\';
				case HierachicalStructureSeperator.SeperatorDot: return '\\';
				case HierachicalStructureSeperator.SeperatorUnderscore: return '\\';
				case HierachicalStructureSeperator.SeperatorPercent: return '%';
				case HierachicalStructureSeperator.SeperatorRecSep: return '\\';
				case HierachicalStructureSeperator.SeperatorThreePlus: return '\\';
				case HierachicalStructureSeperator.SeperatorTab: return '\\';
				default:
					throw new ArgumentOutOfRangeException(nameof(hss), hss, null);
			}
		}
	}
}
