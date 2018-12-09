using System;
using System.Collections.Generic;
using System.Text;
using AlephNote.Common.Repository;
using AlephNote.PluginInterface;

namespace AlephNote.Common.SPSParser
{
	public class SimpleParamStringParser
	{
		private enum ParseMode { Plain, Keyword, Parameter }

		private abstract class SPSException : Exception { }
		private class SyntaxException : SPSException { }

		private class Context { public NoteRepository R; public INote N; }

		private static readonly Random RND = new Random();

		private readonly Dictionary<string, Func<string, string, Context, string>> _keywords = new Dictionary<string, Func<string, string, Context, string>>();

		// ReSharper disable FormatStringProblem
		public SimpleParamStringParser()
		{
			_keywords.Add("now",        (k, p, c) => string.Format("{0:" + (p ?? "yyyy-MM-dd HH:mm:ss") + "}", DateTime.Now));
			_keywords.Add("utcnow",     (k, p, c) => string.Format("{0:" + (p ?? "yyyy-MM-dd HH:mm:ss") + "}", DateTime.UtcNow));
			_keywords.Add("time",       (k, p, c) => string.Format("{0:" + (p ?? "HH:mm") + "}",               DateTime.Now));
			_keywords.Add("date",       (k, p, c) => string.Format("{0:" + (p ?? "yyyy-MM-dd") + "}",          DateTime.Now));

			_keywords.Add("uuid",       (k, p, c) => Guid.NewGuid().ToString(p ?? ""));
			_keywords.Add("guid",       (k, p, c) => Guid.NewGuid().ToString(p ?? ""));

			_keywords.Add("linebreak",  (k, p, c) => Environment.NewLine);
			_keywords.Add("crlf",       (k, p, c) => "\r\n");
			_keywords.Add("cr",         (k, p, c) => "\r");
			_keywords.Add("lf",         (k, p, c) => "\n");
			_keywords.Add("tab",        (k, p, c) => "\t");

			_keywords.Add("random",     (k, p, c) =>
			{
				if (p == null)   return RND.Next().ToString();
				var s = p.Split(',');
				if (s.Length==1) return RND.Next(int.Parse(s[0])).ToString();
				if (s.Length==2) return RND.Next(int.Parse(s[0]), int.Parse(s[1])).ToString();
				throw new Exception("Invalid parameter for {random}");
			});
			
			_keywords.Add("plugin",     (k, p, c) =>
			{
				if (p == null)   return c.R.ConnectionName;
				if (p == "id" || p == "uuid")   return c.R.ProviderID;
				if (p == "name")   return c.R.ConnectionName;
				throw new Exception("Invalid parameter for {plugin}");
			});

			_keywords.Add("account",    (k, p, c) =>
			{
				if (p == null)   return c.R.ConnectionDisplayTitle;
				if (p == "id" || p == "uuid")   return c.R.ConnectionUUID;
				if (p == "name")   return c.R.ConnectionDisplayTitle;
				throw new Exception("Invalid parameter for {account}");
			});

			_keywords.Add("note",       (k, p, c) =>
			{
				if (c.N == null) return string.Empty;
				if (p == null)   return c.N.Title;
				if (p == "id" || p == "uuid") return c.N.UniqueName;
				if (p == "name" || p == "title") return c.N.Title;
				if (p == "cdate") return c.N.CreationDate.ToString("yyyy-MM-dd HH:mm:ss");
				if (p == "mdate") return c.N.ModificationDate.ToString("yyyy-MM-dd HH:mm:ss");
				throw new Exception("Invalid parameter for {note}");
			});
		}
		// ReSharper restore FormatStringProblem

		public string Parse(string input, NoteRepository r, INote n, out bool success)
		{
			try
			{
				var c = new Context { R = r, N = n };

				var str = ParseInternal(input, c);
				success = true;
				return str;
			}
			catch (Exception)
			{
				success = false;
				return input;
			}
		}

		private string ParseInternal(string input, Context c)
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
						builderOut.Append(builderMain);
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
						builderOut.Append(Evaluate(builderMain.ToString(), null, c));
						mode = ParseMode.Plain;
						builderMain.Clear();
						lastKeyword = string.Empty;
					}
					else if (mode == ParseMode.Parameter)
					{
						builderOut.Append(Evaluate(lastKeyword, builderMain.ToString(), c));
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

			builderOut.Append(builderMain);
			return builderOut.ToString();
		}

		private string Evaluate(string keyword, string param, Context c)
		{
			if (_keywords.TryGetValue(keyword.ToLower(), out var func)) return func(keyword, param, c);

			throw new SyntaxException();
		}
	}
}
