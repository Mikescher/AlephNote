using AlephNote.Common.Settings.Types;
using AlephNote.WPF.MVVM;
using System.Collections.Generic;
using System.Linq;

namespace AlephNote.WPF.Converter
{
	public class SnippetElem { public string Header { get; set; } public string Value { get; set; } public string AlephAction { get; set; } }

	class SnippetsToSnippetSource : OneWayConverter<KeyValueCustomList<SnippetDefinition>, List<SnippetElem>>
	{
		protected override List<SnippetElem> Convert(KeyValueCustomList<SnippetDefinition> value, object parameter)
		{
			return value.Data.Select(d => new SnippetElem
			{
				Header      = d.Value.DisplayName,
				Value       = d.Value.Value,
				AlephAction = "Snippet::"+d.Key,
			}).ToList();
		}
	}
}
