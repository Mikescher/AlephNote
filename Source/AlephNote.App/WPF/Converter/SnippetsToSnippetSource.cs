using AlephNote.Common.Settings.Types;
using AlephNote.WPF.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace AlephNote.WPF.Converter
{
	class SnippetsToSnippetSource : OneWayConverter<KeyValueStringList, List<SnippetsToSnippetSource.SnippetElem>>
	{
		public class SnippetElem { public string Header { get; set; } public string Value { get; set; } }

		public SnippetsToSnippetSource() { }

		protected override List<SnippetElem> Convert(KeyValueStringList value, object parameter)
		{
			return value.Data.Select(d => new SnippetElem { Header = d.Item1, Value = d.Item2 }).ToList();
		}
	}
}
