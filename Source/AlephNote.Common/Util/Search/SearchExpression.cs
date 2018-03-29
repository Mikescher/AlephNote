using AlephNote.PluginInterface;
using System.Linq;
using System.Text.RegularExpressions;

namespace AlephNote.Common.Util.Search
{
	public abstract class SearchExpression
	{
		public abstract bool IsMatch(INote n);
	}

	public class SearchExpression_Regex : SearchExpression
	{
		private readonly Regex _regex;

		public SearchExpression_Regex(Regex r)
		{
			_regex = r;
		}

		public override bool IsMatch(INote n)
		{
			if (_regex.IsMatch(n.Title)) return true;
			if (_regex.IsMatch(n.Text)) return true;
			if (n.Tags.Any(t => _regex.IsMatch(t))) return true;

			return false;
		}
	}
	
	public class SearchExpression_Text : SearchExpression
	{
		private readonly string _needle;
		private readonly bool _nocase;
		private readonly bool _exact;

		public SearchExpression_Text(string txt, bool ignorecase, bool exact)
		{
			_needle = txt;
			_nocase = ignorecase;
			_exact = exact;
		}

		public override bool IsMatch(INote n)
		{
			if (_nocase)
			{
				if (_exact)
				{
					if (n.Title.ToLower() == _needle.ToLower()) return true;
					if (n.Text.ToLower() == _needle.ToLower()) return true;
				}
				else
				{
					if (n.Title.ToLower().Contains(_needle.ToLower())) return true;
					if (n.Text.ToLower().Contains(_needle.ToLower())) return true;
				}

				if (n.HasTagCaseInsensitive(_needle)) return true;
			}
			else
			{
				if (_exact)
				{
					if (n.Title == _needle) return true;
					if (n.Text == _needle) return true;
				}
				else
				{
					if (n.Title.Contains(_needle)) return true;
					if (n.Text.Contains(_needle)) return true;
				}

				if (n.HasTagCaseSensitive(_needle)) return true;
			}

			return false;
		}
	}
	
	public class SearchExpression_Tag : SearchExpression
	{
		private readonly string _tag;
		private readonly bool _nocase;

		public SearchExpression_Tag(string tag, bool ignorecase)
		{
			_tag = tag;
			_nocase = ignorecase;
		}

		public override bool IsMatch(INote n)
		{
			if (_nocase  && n.HasTagCaseInsensitive(_tag)) return true;
			if (!_nocase && n.HasTagCaseSensitive(_tag)) return true;

			return false;
		}
	}
	
	public class SearchExpression_All : SearchExpression
	{
		public override bool IsMatch(INote n) => true;
	}
	
	public class SearchExpression_None : SearchExpression
	{
		public override bool IsMatch(INote n) => false;
	}
	
	public class SearchExpression_AND : SearchExpression
	{
		private SearchExpression[] _expressions;

		public SearchExpression_AND(params SearchExpression[] se)
		{
			_expressions = se;
		}

		public override bool IsMatch(INote n) => _expressions.All(e => e.IsMatch(n));
	}
	
	public class SearchExpression_OR : SearchExpression
	{
		private SearchExpression[] _expressions;

		public SearchExpression_OR(params SearchExpression[] se)
		{
			_expressions = se;
		}

		public override bool IsMatch(INote n) => _expressions.Any(e => e.IsMatch(n));
	}
	
	public class SearchExpression_NOT : SearchExpression
	{
		private SearchExpression _expression;

		public SearchExpression_NOT(SearchExpression se)
		{
			_expression = se;
		}

		public override bool IsMatch(INote n) => !_expression.IsMatch(n);
	}
}
