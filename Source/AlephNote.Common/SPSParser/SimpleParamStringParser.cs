using System;
using System.Collections.Generic;
using System.Text;

namespace AlephNote.Common.SPSParser
{
	public class SimpleParamStringParser
	{
		private enum ParseMode { Plain, Keyword, Parameter }

		public abstract class SPSException : Exception { }
		public class SyntaxException : SPSException { }

		private Dictionary<string, Func<string, string, string>> _keywords = new Dictionary<string, Func<string, string, string>>();

		public SimpleParamStringParser()
		{
			_keywords.Add("now",    (k, p) => String.Format("{0:" + (p ?? "yyyy-MM-dd HH:mm:ss") + "}", DateTime.Now));
			_keywords.Add("utcnow", (k, p) => String.Format("{0:" + (p ?? "yyyy-MM-dd HH:mm:ss") + "}", DateTime.UtcNow));
			_keywords.Add("time",   (k, p) => String.Format("{0:" + (p ?? "HH:mm") + "}", DateTime.Now));
			_keywords.Add("date",   (k, p) => String.Format("{0:" + (p ?? "yyyy-MM-dd") + "}", DateTime.Now));
		}

		public string Parse(string input, out bool success)
		{
			try
			{
				var str = ParsInternal(input);
				success = true;
				return str;
			}
			catch (Exception)
			{
				success = false;
				return input;
			}
		}

		private string ParsInternal(string input)
		{
			input = input.Replace(@"\r\n", @"\n");
			input = input.Replace(@"\n", Environment.NewLine);
			input = input.Replace(@"\t", "\t");

			var builderOut = new StringBuilder();
			var builderMain = new StringBuilder();

			ParseMode mode = ParseMode.Plain;
			var lastKeyword = "";

			for (int i = 0; i < input.Length; i++)
			{
				char character = input[i];
				char? next = i + 1 < input.Length ? input[i + 1] : (char?)null;

				if (character == '{')
				{
					if (mode != ParseMode.Plain) throw new SyntaxException();

					if (next == character)
					{
						builderMain.Append(character);
						i++;
					}
					else
					{
						builderOut.Append(builderMain.ToString());
						mode = ParseMode.Keyword;
						builderMain.Clear();
					}
				}
				else if (character == '}')
				{
					if (mode == ParseMode.Plain && next == character)
					{
						builderMain.Append(character);
						i++;
					}
					else if (mode == ParseMode.Keyword)
					{
						builderOut.Append(Evaluate(builderMain.ToString(), null));
						mode = ParseMode.Plain;
						builderMain.Clear();
						lastKeyword = string.Empty;
					}
					else if (mode == ParseMode.Parameter)
					{
						builderOut.Append(Evaluate(lastKeyword, builderMain.ToString()));
						mode = ParseMode.Plain;
						builderMain.Clear();
						lastKeyword = string.Empty;
					}
					else
					{
						throw new SyntaxException();
					}
				}
				else if (character == ':')
				{
					if (mode == ParseMode.Plain)
					{
						builderMain.Append(character);
					}
					else if (mode == ParseMode.Keyword)
					{
						lastKeyword = builderMain.ToString();
						mode = ParseMode.Parameter;
						builderMain.Clear();
					}
					else if (mode == ParseMode.Parameter)
					{
						builderMain.Append(character);
					}
					else
					{
						throw new SyntaxException();
					}
				}
				else
				{
					builderMain.Append(character);
				}
			}

			if (mode != ParseMode.Plain) throw new SyntaxException();

			builderOut.Append(builderMain.ToString());
			return builderOut.ToString();
		}

		private string Evaluate(string keyword, string param)
		{
			Func<string, string, string> func;

			if (_keywords.TryGetValue(keyword.ToLower(), out func))
			{
				return func(keyword, param);
			}
			else
			{
				throw new SyntaxException();
			}
		}
	}
}
