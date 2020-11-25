using System;
using MSHC.Lang.Attributes;

namespace AlephNote.Common.Settings.Types
{
	public enum HierarchicalStructureSeperator
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
		public static string GetSeperator(HierarchicalStructureSeperator hss)
		{
			switch (hss)
			{
				case HierarchicalStructureSeperator.SeperatorForwardSlash: return "/";
				case HierarchicalStructureSeperator.SeperatorRSlash: return "\\";
				case HierarchicalStructureSeperator.SeperatorDot: return ".";
				case HierarchicalStructureSeperator.SeperatorUnderscore: return "_";
				case HierarchicalStructureSeperator.SeperatorPercent: return "%";
				case HierarchicalStructureSeperator.SeperatorRecSep: return "%1E%"; 
				case HierarchicalStructureSeperator.SeperatorThreePlus: return "+++";
				case HierarchicalStructureSeperator.SeperatorTab: return "\t";
				default:
					throw new ArgumentOutOfRangeException(nameof(hss), hss, null);
			}
		}

		public static char GetEscapeChar(HierarchicalStructureSeperator hss)
		{
			switch (hss)
			{
				case HierarchicalStructureSeperator.SeperatorForwardSlash: return '\\';
				case HierarchicalStructureSeperator.SeperatorRSlash: return '\\';
				case HierarchicalStructureSeperator.SeperatorDot: return '\\';
				case HierarchicalStructureSeperator.SeperatorUnderscore: return '\\';
				case HierarchicalStructureSeperator.SeperatorPercent: return '%';
				case HierarchicalStructureSeperator.SeperatorRecSep: return '\\';
				case HierarchicalStructureSeperator.SeperatorThreePlus: return '\\';
				case HierarchicalStructureSeperator.SeperatorTab: return '\\';
				default:
					throw new ArgumentOutOfRangeException(nameof(hss), hss, null);
			}
		}
	}
}
