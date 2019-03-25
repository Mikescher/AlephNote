using AlephNote.PluginInterface.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MSHC.Lang.Collections;

namespace AlephNote.Common.Util.Search
{
	public static class SearchStringParser
	{
		public const string TAG_SEARCH_EMPTY = "[[EMPTY]]";

		private static readonly LimitedDictionary<string, SearchExpression> _cache = new LimitedDictionary<string, SearchExpression>(64);
		
		public static SearchExpression Parse(string searchText)
		{
			if (_cache.TryGetValue(searchText, out var r1)) return r1;

			var r2 = _Parse(searchText);
			_cache[searchText] = r2;
			return r2;
		}

		private static SearchExpression _Parse(string searchText)
		{
			if (string.IsNullOrWhiteSpace(searchText)) return new SearchExpression_All();
			
			//   /regex/
			if (IsRegex(searchText, out var searchRegex)) return new SearchExpression_Regex(searchRegex);

			//   [tag1] [tag2] [tag3]
			if (IsTagList(searchText, out var tagList)) return new SearchExpression_AND(tagList.Select(CreateTagSearchExpr).ToArray());
			
			//   "text"
			if (IsQuoted(searchText)) return new SearchExpression_Text(searchText.Substring(1, searchText.Length-2), true, true);

			//   asdf
			return new SearchExpression_Text(searchText, true, false);
		}

		private static SearchExpression CreateTagSearchExpr(string se)
		{
			if (se.ToLower() == TAG_SEARCH_EMPTY.ToLower()) return new SearchExpression_NoTag();
			return new SearchExpression_Tag(se, true);
		}

		private static bool IsQuoted(string text)
		{
			return (text.Length >= 3 && text.StartsWith("\"") && text.EndsWith("\"") && !text.Substring(1, text.Length-2).Contains("\""));
		}

		public static bool IsRegex(string text, out Regex r)
		{
			try
			{
				if (text.Length >= 3 && text.StartsWith("/") && text.EndsWith("/"))
				{
					try
					{
						r = new Regex(text.Substring(1, text.Length - 2));
						return true;
					}
					catch (ArgumentException)
					{
						r = null;
						return false;
					}
				}
				else
				{
					r = null;
					return false;
				}

			}
			catch (ArgumentException)
			{
				r = null;
				return false;
			}
		}

		private static bool IsTagList(string text, out List<string> r)
		{
			r = new List<string>();

			if (!text.StartsWith("[") || !text.EndsWith("]")) return false;

			var intag = false;
			var intag2 = false;
			var tagbuilder = new StringBuilder();
			for (var i = 0; i < text.Length; i++)
			{
				var chr = text[i];
				var chrnext = (i+1 < text.Length) ? text[i+1] : (char?)null;
				if (intag2)
				{
					if (chr == '\\' && i+1<text.Length && (text[i+1]=='[' || text[i+1]==']' || text[i+1]=='\\') )
					{
						tagbuilder.Append(text[i+1]);
						i++;
					}
					else if (chr == '[')
					{
						return false;
					}
					else if (chr == ']' && chrnext == ']')
					{
						tagbuilder.Append("]]");
						var t = tagbuilder.ToString();
						if (string.IsNullOrWhiteSpace(t)) return false;
						r.Add(t);
						intag2 = false;
						i++;
					}
					else if (chr == ']')
					{
						return false;
					}
					else
					{
						tagbuilder.Append(chr);
					}
				}
				else if (intag)
				{
					if (chr == '\\' && i+1<text.Length && (text[i+1]=='[' || text[i+1]==']' || text[i+1]=='\\') )
					{
						tagbuilder.Append(text[i+1]);
						i++;
					}
					else if (chr == '[')
					{
						return false;
					}
					else if (chr == ']')
					{
						var t = tagbuilder.ToString();
						if (string.IsNullOrWhiteSpace(t)) return false;
						r.Add(t);
						intag = false;
					}
					else
					{
						tagbuilder.Append(chr);
					}
				}
				else
				{
					if (chr=='[')
					{
						if (chrnext == '[')
						{
							intag2=true;
							tagbuilder.Clear();
							tagbuilder.Append("[[");
							i++;
						}
						else
						{
							intag=true;
							tagbuilder.Clear();
						}
					}
					else if (chr == ' ' || chr == '\t')
					{
						continue;
					}
					else
					{
						return false;
					}
				}
			}
			if (intag) return false;
			if (intag2) return false;
			if (r.Count==0) return false;

			return true;
		}

		public static string GetTagSearch(string tagname)
		{
			return "[" + tagname.Replace("\\", "\\\\").Replace("[", "\\[").Replace("]", "\\]") + "]";
		}
	}
}
