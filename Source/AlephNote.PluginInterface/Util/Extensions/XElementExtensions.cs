using System;
using System.Globalization;
using System.Xml.Linq;

namespace AlephNote.PluginInterface.Util
{
	public static class XElementExtensions
	{
		#region Attribute Accessor

		public static string StringAttribute(this XElement element, string key, string defaultValue)
		{
			var attr = element.Attribute(key);
			if (attr != null) return attr.Value;

			return defaultValue;
		}

		public static string StringAttribute(this XElement element, string key)
		{
			var attr = element.Attribute(key);
			if (attr != null) return attr.Value;

			throw new XMLStructureException(string.Format("Attribute {0} not found on element {1}", key, element.Name.LocalName));
		}

		public static double DoubleAttribute(this XElement element, string key, double defaultValue)
		{
			var attr = element.Attribute(key);
			if (attr != null)
			{
				double temp;
				if (double.TryParse(attr.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out temp)) return temp;
			}

			return defaultValue;
		}

		public static double DoubleAttribute(this XElement element, string key)
		{
			var attr = element.Attribute(key);
			if (attr != null)
			{
				double temp;
				if (double.TryParse(attr.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out temp)) return temp;
			}

			throw new XMLStructureException(string.Format("Attribute {0} not found on element {1}", key, element.Name.LocalName));
		}

		public static int IntAttribute(this XElement element, string key, int defaultValue)
		{
			var attr = element.Attribute(key);
			if (attr != null)
			{
				int temp;
				if (int.TryParse(attr.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out temp)) return temp;
			}

			return defaultValue;
		}

		public static int IntAttribute(this XElement element, string key)
		{
			var attr = element.Attribute(key);
			if (attr != null)
			{
				int temp;
				if (int.TryParse(attr.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out temp)) return temp;
			}

			throw new XMLStructureException(string.Format("Attribute {0} not found on element {1}", key, element.Name.LocalName));
		}

		public static bool BoolAttribute(this XElement element, string key)
		{
			var attr = element.Attribute(key);
			if (attr != null)
			{
				return ParseBool(attr.Value);
			}

			throw new XMLStructureException(string.Format("Attribute {0} not found on element {1}", key, element.Name.LocalName));
		}

		public static bool BoolAttribute(this XElement element, string key, bool defaultValue)
		{
			var attr = element.Attribute(key);
			if (attr != null)
			{
				bool? temp = TryParseBool(attr.Value);
				if (temp != null) return temp.Value;
			}

			return defaultValue;
		}

		public static Guid GuidAttribute(this XElement element, string key)
		{
			var attr = element.Attribute(key);
			if (attr != null)
			{
				Guid temp;
				if (Guid.TryParse(attr.Value, out temp)) return temp;
			}

			throw new XMLStructureException(string.Format("Attribute {0} not found on element {1}", key, element.Name.LocalName));
		}

		#endregion

		public static bool ParseBool(string value)
		{
			var b = TryParseBool(value);

			if (b == null) throw new XMLStructureException("Unparseable boolean value: " + value);

			return b.Value;
		}

		public static bool? TryParseBool(string value)
		{
			if (value == null) return null;

			if (value.ToLower() == "true") return true;
			if (value.ToLower() == "false") return false;

			if (value.ToLower() == "1") return true;
			if (value.ToLower() == "0") return false;

			return null;
		}
	}
}
