using System.Collections.Generic;
using System.Text;

namespace AlephNote.PluginInterface.Util
{
	public class HierachyEmulationConfig
	{
		public readonly bool EmulateSubfolders;
		public readonly string SeperatorString;
		public readonly char EscapeChar;

		public HierachyEmulationConfig(bool enabled, string seperator, char escape)
		{
			EmulateSubfolders = enabled;
			SeperatorString = seperator;
			EscapeChar = escape;
		}

		public string EscapeStringForRemote(string str)
		{
			if (EscapeChar.ToString() == SeperatorString)
			{
				return str.Replace(EscapeChar.ToString(), EscapeChar + "" + EscapeChar);
			}
			else
			{
				str = str.Replace(EscapeChar.ToString(), EscapeChar + "" + EscapeChar);
				str = str.Replace(SeperatorString, EscapeChar + SeperatorString);
				return str;
			}
		}

		public string EscapeStringListForRemote(IEnumerable<string> list)
		{
			var b = new StringBuilder();

			bool first = true;
			foreach (var elem in list)
			{
				if (!first) b.Append(SeperatorString);
				first = false;

				b.Append(EscapeStringForRemote(elem));
			}
			return b.ToString();
		}

		public IEnumerable<string> UnescapeStringFromRemote(string str)
		{
			var b = new StringBuilder();

			var escape = false;
			for (int i = 0; i < str.Length; i++)
			{
				if (escape)
				{
					b.Append(str[i]);
					continue;
				}
				else
				{
					if (str[i] == EscapeChar)
					{
						if (EscapeChar.ToString() == SeperatorString && i+1 < str.Length && !str.Substring(i+1).StartsWith(SeperatorString))
						{
							i += SeperatorString.Length - 1;
							yield return b.ToString();
							b.Clear();
							continue;
						}
						else
						{
							escape = true;
							continue;
						}
					}
					else
					{
						if (str[i] == SeperatorString[0] && str.Substring(i).StartsWith(SeperatorString))
						{
							i += SeperatorString.Length - 1;
							yield return b.ToString();
							b.Clear();
							continue;
						}
						else
						{
							b.Append(str[i]);
							continue;
						}
					}
				}
			}
			yield return b.ToString();
			yield break;

		}
	}
}
