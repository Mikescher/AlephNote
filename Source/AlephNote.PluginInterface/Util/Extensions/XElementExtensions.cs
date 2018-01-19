using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

		#region ParseBool

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

		#endregion

		#region XList

		public static string XListSingleOrDefault(this XContainer x, params string[] p)
		{
			return XList(x, p).FirstOrDefault();
		}

		public static string XListSingle(this XContainer x, params string[] p)
		{
			var r = XList(x, p).FirstOrDefault();
			if (r == null) throw new Exception("XML Entry not found: " + string.Join(" --> ", p.Select(pp => $"[{pp}]")));
			return r;
		}

		public static IEnumerable<string> XList(this XContainer x, params string[] p)
		{
			if (p.Last().StartsWith("."))
			{
				var last = p.Last();
				var search = last.Substring(1);

				foreach (var elem in XElemList(x, p.Reverse().Skip(1).Reverse().ToArray()))
				{
					foreach (var attr in elem.Attributes().Where(e => e.Name.LocalName.ToLower() == search.ToLower()))
					{
						yield return attr.Value;
					}
				}
			}
			else
			{
				foreach (var elem in XElemList(x, p))
				{
					yield return elem.Value;
				}
			}
		}

		/// <summary>
		/// Reserved Chars:  * . @ ~ & = 
		/// 
		/// XML-Tag by name (case-insensitive):
		///     "asdf"
		/// 
		/// None,One,Many tags by name (case-insensitive):
		///     "*asdf*"
		/// 
		/// Tag with attribute+value
		///     "asdf@attr=value"
		/// 
		/// Tag with multiple attribute+value
		///     "asdf@attr=value&other=umts"
		/// 
		/// Tag with attribute + any value
		///     "asdf@attr=~"
		/// 
		/// Query for attribute value (only in XList)
		///     ".attr"
		/// 
		/// </summary>
		public static IEnumerable<XElement> XElemList(this XContainer x, params string[] p)
		{
			var search = p[0];
			var nn = p.Skip(1).ToArray();

			var searchMulti = false;
			if (search.Length > 2 && search.StartsWith("*") && search.EndsWith("*"))
			{
				search = search.Substring(1, search.Length - 2);
				searchMulti = true;
			}

			List<Tuple<string, string>> attrFilter = new List<Tuple<string, string>>();
			if (search.Contains('@'))
			{
				var split = search.Split('@');

				search = split[0];

				foreach (var filter in split[1].Split('&'))
				{
					attrFilter.Add(Tuple.Create(filter.Split('=')[0], filter.Split('=')[1]));
				}
			}
			
			var xf = x.Elements().Where(e => e.Name.LocalName.ToLower() == search.ToLower());

			foreach (var filter in attrFilter)
			{
				xf = xf.Where(xx => xx.Attributes().Any(e => e.Name.LocalName.ToLower() == filter.Item1.ToLower()));
				if (filter.Item2 != "~") xf = xf.Where(xx => xx.Attributes().Where(e => e.Name.LocalName.ToLower() == filter.Item1.ToLower()).Any(attr => attr.Value == filter.Item2));
			}

			if (nn.Length == 0)
			{
				foreach (var f in xf) yield return f;
			}

			if (nn.Length > 0) foreach (var f in xf) foreach (var rf in XElemList(f, nn)) yield return rf;
			if (searchMulti) foreach (var f in xf) foreach (var rf in XElemList(f, p)) yield return rf;
		}

		#endregion
	}
}
