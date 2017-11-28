using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace AlephNote.PluginInterface.Util
{
	public static class XHelper
	{
		#region GetChildValue optional

		public static string GetChildValue(XElement parent, string childName, string defaultValue)
		{
			var child = parent.Elements(childName).FirstOrDefault();
			if (child == null) return defaultValue;

			return child.Value;
		}

		public static int GetChildValue(XElement parent, string childName, int defaultValue)
		{
			var child = parent.Elements(childName).FirstOrDefault();
			if (child == null) return defaultValue;

			return int.Parse(child.Value);
		}

		public static int? GetChildValue(XElement parent, string childName, int? defaultValue)
		{
			var child = parent.Elements(childName).FirstOrDefault();
			if (child == null) return defaultValue;

			if (child.Value.Trim() == "") return null;

			return int.Parse(child.Value);
		}

		public static bool GetChildValue(XElement parent, string childName, bool defaultValue)
		{
			var child = parent.Elements(childName).FirstOrDefault();
			if (child == null) return defaultValue;

			return XElementExtensions.ParseBool(child.Value);
		}

		public static Guid GetChildValue(XElement parent, string childName, Guid defaultValue)
		{
			var child = parent.Elements(childName).FirstOrDefault();
			if (child == null) return defaultValue;

			return Guid.Parse(child.Value);
		}

		public static Guid? GetChildValue(XElement parent, string childName, Guid? defaultValue)
		{
			var child = parent.Elements(childName).FirstOrDefault();
			if (child == null) return defaultValue;
			if (string.IsNullOrWhiteSpace(child.Value)) return defaultValue;

			return Guid.Parse(child.Value);
		}

		public static double GetChildValue(XElement parent, string childName, double defaultValue)
		{
			var child = parent.Elements(childName).FirstOrDefault();
			if (child == null) return defaultValue;

			return double.Parse(child.Value);
		}

		public static TEnumType GetChildValue<TEnumType>(XElement parent, string childName, TEnumType defaultValue) where TEnumType : struct, IComparable, IFormattable, IConvertible
		{
			var child = parent.Elements(childName).FirstOrDefault();
			if (child == null) return defaultValue;

			int value;
			TEnumType evalue;
			if (int.TryParse(child.Value, out value))
			{
				foreach (var enumValue in Enum.GetValues(typeof(TEnumType)))
				{
					if (value == Convert.ToInt32(Enum.Parse(typeof(TEnumType), enumValue.ToString())))
						return (TEnumType)enumValue;
				}
			}
			if (Enum.TryParse(child.Value, true, out evalue))
			{
				return evalue;
			}

			throw new ArgumentException("'" + child.Value + "' is not a valid value for Enum");
		}

		public static object GetChildValue(XElement parent, string childName, object defaultValue, Type enumType)
		{
			var child = parent.Elements(childName).FirstOrDefault();
			if (child == null) return defaultValue;

			int value;
			if (int.TryParse(child.Value, out value))
			{
				foreach (var enumValue in Enum.GetValues(enumType))
				{
					if (value == Convert.ToInt32(Enum.Parse(enumType, enumValue.ToString())))
						return enumValue;
				}
			}

			return Enum.Parse(enumType, child.Value);
		}

		#endregion

		#region GetChildValue

		public static string GetChildValueString(XElement parent, string childName)
		{
			return GetChildOrThrow(parent, childName).Value;
		}

		public static string GetChildBase64String(XElement parent, string childName)
		{
			return ConvertFromC80Base64(GetChildOrThrow(parent, childName).Value);
		}

		public static int GetChildValueInt(XElement parent, string childName)
		{
			var child = GetChildOrThrow(parent, childName);

			return int.Parse(child.Value);
		}

		public static int? GetChildValueNint(XElement parent, string childName)
		{
			var child = GetChildOrThrow(parent, childName);

			if (child.Value.Trim() == "") return null;

			return int.Parse(child.Value);
		}

		public static bool GetChildValueBool(XElement parent, string childName)
		{
			var child = GetChildOrThrow(parent, childName);

			return XElementExtensions.ParseBool(child.Value);
		}

		public static Guid GetChildValueGUID(XElement parent, string childName)
		{
			var child = GetChildOrThrow(parent, childName);

			return Guid.Parse(child.Value);
		}

		public static TEnumType GetChildValueEnum<TEnumType>(XElement parent, string childName) where TEnumType : struct, IComparable, IFormattable, IConvertible
		{
			var child = GetChildOrThrow(parent, childName);

			int value;
			TEnumType evalue;
			if (int.TryParse(child.Value, out value))
			{
				foreach (var enumValue in Enum.GetValues(typeof(TEnumType)))
				{
					if (value == Convert.ToInt32(Enum.Parse(typeof(TEnumType), enumValue.ToString())))
						return (TEnumType)enumValue;
				}
			}
			if (Enum.TryParse(child.Value, true, out evalue))
			{
				return evalue;
			}

			throw new ArgumentException("'" + child.Value + "' is not a valid value for Enum");
		}

		public static List<string> GetChildValueStringList(XElement parent, string childName, string subNodeName)
		{
			var child = GetChildOrThrow(parent, childName);

			return child.Elements(subNodeName).Select(p => p.Value).ToList();
		}

		public static DateTimeOffset GetChildValueDateTimeOffset(XElement parent, string childName)
		{
			var child = GetChildOrThrow(parent, childName);

			return DateTimeOffset.Parse(child.Value);
		}

		#endregion

		public static XElement GetChildOrThrow(XElement parent, string childName)
		{
			var child = parent.Elements(childName).FirstOrDefault();
			if (child == null) throw new XMLStructureException("Node not found: " + childName);
			return child;
		}

		public static XElement GetChildOrNull(XElement parent, string childName)
		{
			return parent.Elements(childName).FirstOrDefault();
		}

		public static string ConvertToString(XDocument doc)
		{
			if (doc == null) throw new ArgumentNullException("doc");

			StringBuilder builder = new StringBuilder();
			using (TextWriter writer = new StringWriter(builder))
			{
				doc.Save(writer);
			}

			var lines = Regex.Split(builder.ToString(), @"\r?\n");
			if (lines.Any()) lines[0] = lines[0].Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "<?xml version=\"1.0\" encoding=\"utf-8\"?>");
			return string.Join(Environment.NewLine, lines);
		}

		public static string ConvertToC80Base64(string content, int indent = 8, int indentLast = 6)
		{
			var chunks = ChunkSplit(Convert.ToBase64String(Encoding.UTF8.GetBytes(content)), 80).ToList();

			if (chunks.Count == 1) return chunks[0];

			var i1 = new string(' ', indent);
			var i2 = new string(' ', indentLast);
			return "\n" + string.Join("", chunks.Select(c => i1 + c + '\n')) + i2;
		}

		public static string ConvertFromC80Base64(string content)
		{
			return Encoding.UTF8.GetString(Convert.FromBase64String(content.Replace("\r", "").Replace("\n", "").Replace(" ", "").Replace("\t", "")));
		}

		public static IEnumerable<string> ChunkSplit(string str, int maxChunkSize)
		{
			for (int i = 0; i < str.Length; i += maxChunkSize)
				yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
		}

		#region GetAttribute

		public static Guid GetAttributeGuid(XElement elem, string attrname)
		{
			var attr = elem.Attribute(attrname);
			if (attr == null) throw new XMLStructureException("Attribute not found: " + attrname);
			return Guid.Parse(attr.Value);
		}

		public static Guid? GetAttributeNGuid(XElement elem, string attrname)
		{
			var attr = elem.Attribute(attrname);
			if (attr == null) throw new XMLStructureException("Attribute not found: " + attrname);
			if (string.IsNullOrWhiteSpace(attr.Value) || attr.Value.ToLower() == "null") return null;
			return Guid.Parse(attr.Value);
		}

		public static string GetAttributeString(XElement elem, string attrname)
		{
			var attr = elem.Attribute(attrname);
			if (attr == null) throw new XMLStructureException("Attribute not found: " + attrname);
			return attr.Value;
		}

		public static int GetAttributeInt(XElement elem, string attrname)
		{
			var attr = elem.Attribute(attrname);
			if (attr == null) throw new XMLStructureException("Attribute not found: " + attrname);
			return int.Parse(attr.Value);
		}

		#endregion
	}
}
