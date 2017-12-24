using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
namespace AlephNote.Commandline
{
	public class CommandLineArguments
	{
		private readonly Dictionary<string, string> paramDict;

		public CommandLineArguments(string[] args, bool caseSensitive = true)
		{
			if (caseSensitive)
				paramDict = new Dictionary<string, string>(StringComparer.CurrentCulture);
			else
				paramDict = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);


			var rexSplitter = new Regex(@"^-{1,2}|^/|=|:", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			var rexRemover = new Regex(@"^['""]?(.*?)['""]?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

			string parameter = null;

			foreach (string txt in args)
			{
				var parts = rexSplitter.Split(txt, 3);

				switch (parts.Length)
				{
					case 1:
						if (parameter != null)
						{
							if (!paramDict.ContainsKey(parameter))
							{
								parts[0] = rexRemover.Replace(parts[0], "$1");
								paramDict.Add(parameter, parts[0]);
							}
							parameter = null;
						}
						break;

					case 2:
						if (parameter != null)
						{
							if (!paramDict.ContainsKey(parameter))
								paramDict.Add(parameter, "true");
						}
						parameter = parts[1];
						break;

					case 3:
						if (parameter != null)
						{
							if (!paramDict.ContainsKey(parameter))
								paramDict.Add(parameter, "true");
						}

						parameter = parts[1];

						if (!paramDict.ContainsKey(parameter))
						{
							parts[2] = rexRemover.Replace(parts[2], "$1");
							paramDict.Add(parameter, parts[2]);
						}

						parameter = null;
						break;
				}
			}
			if (parameter != null)
			{
				if (!paramDict.ContainsKey(parameter))
					paramDict.Add(parameter, "true");
			}
		}

		public bool Contains(string key)
		{
			return paramDict.ContainsKey(key);
		}

		public bool IsSet(string key)
		{
			return paramDict.ContainsKey(key) && paramDict[key] != null;
		}

		public string this[string param] { get { return paramDict[param]; } }

		public bool IsEmpty()
		{
			return paramDict.Count == 0;
		}

		#region String

		public string GetStringDefault(string p, string def)
		{
			return Contains(p) ? this[p] : def;
		}

		public List<string> GetStringList(string p, string delimiter, StringSplitOptions options = StringSplitOptions.None)
		{
			if (Contains(p))
				return this[p].Split(new[] { delimiter }, options).ToList();
			else
				return null;
		}

		#endregion

		#region Long

		public bool IsLong(string p)
		{
			long a;
			return IsSet(p) && long.TryParse(paramDict[p], out a);
		}

		public long GetLong(string p)
		{
			return long.Parse(this[p]);
		}

		public long GetLongDefault(string p, long def)
		{
			return IsLong(p) ? GetLong(p) : def;
		}

		public long? GetLongDefaultNull(string p)
		{
			return IsLong(p) ? GetLong(p) : (long?)null;
		}

		public long GetLongDefaultRange(string p, long def, long min, long max)
		{
			return System.Math.Min(max - 1, System.Math.Max(min, (IsLong(p) ? GetLong(p) : def)));
		}

		public List<long> GetLongList(string p, string delimiter, bool sanitize = false)
		{
			List<String> ls = GetStringList(p, delimiter, sanitize ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);

			long aout;
			if (ls.Any(pp => !long.TryParse(pp, out aout)))
				return null;

			return ls.Select(long.Parse).ToList();
		}

		#endregion

		#region Integer

		public bool IsInt(string p)
		{
			int a;
			return IsSet(p) && int.TryParse(paramDict[p], out a);
		}

		public int GetInt(string p)
		{
			return int.Parse(this[p]);
		}

		public int GetIntDefault(string p, int def)
		{
			return IsInt(p) ? GetInt(p) : def;
		}

		public int? GetIntDefaultNull(string p)
		{
			return IsInt(p) ? GetInt(p) : (int?)null;
		}

		public int GetIntDefaultRange(string p, int def, int min, int max)
		{
			return System.Math.Min(max - 1, System.Math.Max(min, (IsInt(p) ? GetInt(p) : def)));
		}

		public List<int> GetIntList(string p, string delimiter, bool sanitize = false)
		{
			List<String> ls = GetStringList(p, delimiter, sanitize ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);

			int aout;
			if (ls.Any(pp => !int.TryParse(pp, out aout)))
				return null;

			return ls.Select(int.Parse).ToList();
		}

		#endregion

		#region UInteger

		public bool IsUInt(string p)
		{
			uint a;
			return IsSet(p) && uint.TryParse(paramDict[p], out a);
		}

		public uint GetUInt(string p)
		{
			return uint.Parse(this[p]);
		}

		public uint GetUIntDefault(string p, uint def)
		{
			return IsUInt(p) ? GetUInt(p) : def;
		}

		public uint? GetUIntDefaultNull(string p)
		{
			return IsUInt(p) ? GetUInt(p) : (uint?)null;
		}

		public uint GetUIntDefaultRange(string p, uint def, uint min, uint max)
		{
			return System.Math.Min(max - 1, System.Math.Max(min, (IsUInt(p) ? GetUInt(p) : def)));
		}

		public List<uint> GetUIntList(string p, string delimiter, bool sanitize = false)
		{
			List<String> ls = GetStringList(p, delimiter, sanitize ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);

			uint aout;
			if (ls.Any(pp => !uint.TryParse(pp, out aout)))
				return null;

			return ls.Select(uint.Parse).ToList();
		}

		#endregion

		#region Enum

		private bool TryParseEnum<T>(string input, out T value) where T : struct, IConvertible
		{
			T result;
			if (Enum.TryParse(input, true, out result))
			{
				value = result;
				return true;
			}

			int resultOrd;
			if (int.TryParse(input, out resultOrd))
			{
				value =(T)Enum.ToObject(typeof(T), resultOrd);
				return true;
			}

			value = default(T);
			return false;
		}

		private T ParseEnum<T>(string p) where T : struct, IConvertible
		{
			T value;
			if (TryParseEnum(p, out value))
				return value;

			throw new FormatException(string.Format("The value {0} is not a valid enum member", p));
		}

		public bool IsEnum<T>(string p) where T : struct, IConvertible
		{
			if (!IsSet(p)) return false;

			T tmp;
			return TryParseEnum(p, out tmp);
		}

		public T GetEnum<T>(string p) where T : struct, IConvertible
		{
			T value;
			if (TryParseEnum(p, out value))
				return value;

			throw new FormatException(string.Format("The parameter {0} is not a valid enum value", p));
		}

		public T GetEnumDefault<T>(string p, T def) where T : struct, IConvertible
		{
			T value;
			if (TryParseEnum(p, out value))
				return value;
			else
				return def;
		}

		public List<T> GetEnumList<T>(string p, string delimiter, bool sanitize = false) where T : struct, IConvertible
		{
			var ls = GetStringList(p, delimiter, sanitize ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);

			T aout;
			if (ls.Any(pp => !TryParseEnum(pp, out aout)))
				return null;

			return ls.Select(ParseEnum<T>).ToList();
		}

		#endregion

		#region Bool

		private bool TryParseBool(string input, bool allowOrdinal, out bool value)
		{
			if (input.ToLower() == "true")
			{
				value = true;
				return true;
			}

			if (input.ToLower() == "false")
			{
				value = true;
				return true;
			}

			int resultOrd;
			if (allowOrdinal && int.TryParse(input, out resultOrd))
			{
				value = (resultOrd != 0);
				return true;
			}

			value = default(bool);
			return false;
		}

		private bool ParseBool(string p, bool allowOrdinal = false)
		{
			bool value;
			if (TryParseBool(p, allowOrdinal, out value))
				return value;

			throw new FormatException(string.Format("The value {0} is not a valid boolean value", p));
		}

		public bool IsBool(string p, bool allowOrdinal = false)
		{
			bool a;
			return IsSet(p) && TryParseBool(paramDict[p], allowOrdinal, out a);
		}

		public bool GetBool(string p, bool allowOrdinal = false)
		{
			return ParseBool(this[p], allowOrdinal);
		}

		public bool GetBoolDefault(string p, bool def, bool allowOrdinal = false)
		{
			return IsBool(p, allowOrdinal) ? GetBool(p, allowOrdinal) : def;
		}

		public bool? GetBoolDefaultNull(string p, bool allowOrdinal = false)
		{
			return IsBool(p, allowOrdinal) ? GetBool(p, allowOrdinal) : (bool?)null;
		}

		public List<bool> GetBoolList(string p, string delimiter, bool allowOrdinal = false, bool sanitize = false)
		{
			var ls = GetStringList(p, delimiter, sanitize ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);

			bool aout;
			if (ls.Any(pp => !TryParseBool(pp, allowOrdinal, out aout)))
				return null;

			return ls.Select(e => ParseBool(e, allowOrdinal)).ToList();
		}

		#endregion

		#region Version

		public Version GetVersionDefault(string p, Version def)
		{
			if (!Contains(p)) return def;

			if (Version.TryParse(this[p], out var v)) return v;

			return def;
		}

		#endregion

	}
}
