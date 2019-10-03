using System;
using System.Linq;
using System.Text.RegularExpressions;
using AlephNote.Common.Settings;
using AlephNote.Common.Settings.Types;

namespace AlephNote.WPF.ScintillaUtil
{
	public class MarkdownHighlighter : ScintillaHighlighter
	{
		private enum ParserMode { None, Bold, Italic, CodeTick }
		private enum ParserSuperMode { None, FencedTildeBlock, FencedBacktickBlock }

		public override void Highlight(ScintillaNET.Scintilla sci, int istart, int iend, AppSettings s)
		{
			// http://spec.commonmark.org/0.28/

			int visualStart = istart;
			for (; visualStart > 0; visualStart--) if (visualStart > 0 && sci.GetCharAt(visualStart - 1) == '\n') break;
			int visualEnd = iend;

			var fulltext = sci.Text;
			var maxLen = fulltext.Length;

			var start = 0;
			var active = false;
			var smode = ParserSuperMode.None;
			for (;;)
			{
				var lineEnd = Math.Min(start + 1, maxLen);
				while (lineEnd < maxLen && sci.GetCharAt(lineEnd) != '\n') lineEnd++;
				if (lineEnd < maxLen) lineEnd++;

				var text = fulltext.Substring(start, lineEnd - start);

				var isTildeFence = text.Trim().StartsWith("~~~");
				var isTickFence  = text.Trim().StartsWith("```");

				bool resetFenceBlock = false;

				if (smode == ParserSuperMode.None && isTildeFence) smode = ParserSuperMode.FencedTildeBlock;
				else if (smode == ParserSuperMode.FencedTildeBlock && isTildeFence) resetFenceBlock = true;
				else if (smode == ParserSuperMode.None && isTickFence) smode = ParserSuperMode.FencedBacktickBlock;
				else if (smode == ParserSuperMode.FencedBacktickBlock && isTickFence) resetFenceBlock = true;

				if (!active && visualStart < lineEnd)
				{
					sci.StartStyling(start);
					active = true;
				}

				if (active)
				{
					if (smode == ParserSuperMode.FencedBacktickBlock || smode == ParserSuperMode.FencedTildeBlock)
						HighlightFencedLine(sci, text);
					else if (smode == ParserSuperMode.None)
						HighlightLine(sci, text, s);
				}

				if (resetFenceBlock) smode = ParserSuperMode.None;

				if (lineEnd >= visualEnd) return;

				start = lineEnd;
			}
		}

		private void HighlightFencedLine(ScintillaNET.Scintilla sci, string text)
		{
			sci.SetStyling(text.Length, STYLE_MD_CODE);
		}

		private void HighlightLine(ScintillaNET.Scintilla sci, string text, AppSettings s)
		{
			if (text.TrimStart().StartsWith("#") && text.TrimStart().TrimStart('#').StartsWith(" "))
			{
				sci.SetStyling(text.Length, STYLE_MD_HEADER);
				return;
			}

			if (text.StartsWith("    "))
			{
				sci.SetStyling(text.Length, STYLE_MD_CODE);
				return;
			}

			var position = 0;
			var mode = ParserMode.None;
			bool isLineStart = true;
			for (int i = 0; i < text.Length; i++)
			{
				var enumNumLen = 0;

				var isDoubleStar = (i + 2 < text.Length) && text[i] == '*' && text[i + 1] == '*';
				var isSingleStar = !isDoubleStar && (i + 1 < text.Length) && text[i] == '*';
				var isBacktick   = text[i] == '`';
				var isOpenBrace  = text[i] == '[';
				var isEnumStar   = isLineStart && (i + 1 < text.Length) && text[i] == '*' && (text[i + 1] == ' ' || text[i + 1] == '\t');
				var isEnumDash   = isLineStart && (i + 1 < text.Length) && text[i] == '-' && (text[i + 1] == ' ' || text[i + 1] == '\t');
				var isEnumPlus   = isLineStart && (i + 1 < text.Length) && text[i] == '+' && (text[i + 1] == ' ' || text[i + 1] == '\t');
				var isEnumNumber = isLineStart && ParseMarkdownEnumNumber(text.Substring(i), out enumNumLen);

				if (text[i] != ' ' && text[i] != '\t') isLineStart = false;

				if (mode == ParserMode.None)
				{
					if (isDoubleStar)
					{
						sci.SetStyling(i - position, STYLE_MD_DEFAULT);
						position += (i - position);
						mode = ParserMode.Bold;
						continue;
					}
					else if (isSingleStar)
					{
						sci.SetStyling(i - position, STYLE_MD_DEFAULT);
						position += (i - position);
						mode = ParserMode.Italic;
						continue;
					}
					else if (isBacktick)
					{
						sci.SetStyling(i - position, STYLE_MD_DEFAULT);
						position += (i - position);
						mode = ParserMode.CodeTick;
						continue;
					}
					else if (isOpenBrace)
					{
						var found = ParseMarkdownLink(text.Substring(i), s.LinkMode, out var len1, out var len2, out var len3);
						if (found)
						{
							sci.SetStyling(i - position, STYLE_MD_DEFAULT);
							position += (i - position);

							sci.SetStyling(len1, STYLE_MD_URL);
							if (len2>0) sci.SetStyling(len2, STYLE_MD_CLICKURL);
							sci.SetStyling(len3, STYLE_MD_URL);

							position += len1+len2+len3;
							i = position;
							continue;
						}
					}
					else if (isEnumStar || isEnumDash || isEnumPlus)
					{
						sci.SetStyling(i - position, STYLE_MD_DEFAULT);
						position += (i - position);

						sci.SetStyling(2, STYLE_MD_LIST);
						position += 2;
						i = position-1;

						mode = ParserMode.None;
						continue;
					}
					else if (isEnumNumber)
					{
						sci.SetStyling(i - position, STYLE_MD_DEFAULT);
						position += (i - position);

						sci.SetStyling(enumNumLen, STYLE_MD_LIST);
						position += enumNumLen;
						i = position-1;
						continue;
					}
				}
				else if (mode == ParserMode.Bold)
				{
					if (isDoubleStar)
					{
						i++;
						sci.SetStyling(i - position + 1, STYLE_MD_BOLD);
						position += (i - position + 1);
						mode = ParserMode.None;
						continue;
					}
				}
				else if (mode == ParserMode.Italic)
				{
					if (isSingleStar)
					{
						sci.SetStyling(i - position + 1, STYLE_MD_ITALIC);
						position += (i - position + 1);
						mode = ParserMode.None;
						continue;
					}
				}
				else if (mode == ParserMode.CodeTick)
				{
					if (isBacktick)
					{
						sci.SetStyling(i - position + 1, STYLE_MD_CODE);
						position += (i - position + 1);
						mode = ParserMode.None;
						continue;
					}
				}
			}

			sci.SetStyling(text.Length - position, STYLE_MD_DEFAULT);
		}

		private bool ParseMarkdownEnumNumber(string v, out int numlen)
		{
			numlen = -1;

			if (v.Length == 0) return false;
			if (!char.IsDigit(v[0])) return false;

			for (int i = 1; i < v.Length - 1; i++)
			{
				if (char.IsDigit(v[i])) continue;

				if (v[i] == '.' || v[i] == ')')
				{
					if (v[i + 1] == ' ' || v[i + 1] == '\t')
					{
						numlen = i + 2;
						return true;
					}
					return false;
				}

				return false;
			}

			return false;
		}

		private bool ParseMarkdownLink(string v, LinkHighlightMode lm, out int lenPrefix, out int lenCore, out int lenSuffix)
		{
			var i = 0;

			var posCoreStart = -1;
			var posCoreEnd = -1;

			lenPrefix = lenCore = lenSuffix = -1;

			if (v.Length == 0) return false;
			if (v[0] != '[') return false;
			i++;

			for (;;)
			{
				if (i >= v.Length) return false;
				if (v[i] == '[') return false;
				if (v[i] == ']') break;

				i++;
			}

			i++;
			if (i >= v.Length) return false;
			if (v[i] != '(') return false;
			posCoreStart = i;
			i++;

			for (; ; )
			{
				if (i >= v.Length) return false;
				if (v[i] == '(') return false;
				if (v[i] == ')')
				{
					posCoreEnd = i;
					break;
				}

				i++;
			}
			i++;

			while (posCoreStart < i-1 && v[posCoreStart + 1] == ' ') posCoreStart++;
			while (posCoreEnd   > 0   && v[posCoreEnd   - 1] == ' ') posCoreEnd--;
			
			lenPrefix = posCoreStart+1;
			lenCore   = posCoreEnd-lenPrefix;
			lenSuffix = i - (lenPrefix+lenCore);

			if (lm == LinkHighlightMode.Disabled)
			{
				lenPrefix += lenCore;
				lenCore = 0;
			}
			else
			{
				var core = v.Substring(lenPrefix, lenCore);
				var urlmatch = GetURLMatchingRegex().Match(core);
				if (urlmatch.Success && urlmatch.Groups[0].Value == core)
				{
					// url found ... all is good
				}
				else
				{
					lenPrefix += lenCore;
					lenCore = 0;
				}
			}


			return true;
		}

		public override string GetClickedLink(string text, int pos)
		{
			var (line, lineStart, lineLen) = GetLine(text, pos);

			for (;;)
			{
				var idx = line.IndexOf('[');
				if (idx == -1) return null;
				line = line.Substring(idx);
				lineStart += idx;
				
				var found = ParseMarkdownLink(line, LinkHighlightMode.OnlyHighlight, out var lenPrefix, out var lenCore, out var lenSuffix);
				if (found && lenCore > 0 && lineStart+lenPrefix <= pos && pos <= lineStart+lenPrefix+lenCore)
				{
					return line.Substring(lenPrefix, lenCore);
				}

				line = line.Substring(lenPrefix+lenCore+lenSuffix);
				lineStart += lenPrefix+lenCore+lenSuffix;
			}
		}
	}
}
