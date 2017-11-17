using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Impl;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Plugins.Evernote
{
	public class EvernoteNote : BasicNote
	{
		private Guid _id;
		public Guid ID { get { return _id; } set { _id = value; OnPropertyChanged(); } }

		private string _text = "";
		public override string Text { get { return _text; } set { _text = value; OnPropertyChanged(); } }

		private string _title = "";
		public override string Title { get { return _title; } set { _title = value; OnPropertyChanged(); } }

		private DateTimeOffset _creationDate = DateTimeOffset.Now;
		public override DateTimeOffset CreationDate { get { return _creationDate; } set { _creationDate = value; OnPropertyChanged(); } }

		private DateTimeOffset _modificationDate = DateTimeOffset.Now;
		public override DateTimeOffset ModificationDate { get { return _modificationDate; } set { _modificationDate = value; OnPropertyChanged(); } }

		private readonly ObservableCollection<string> _tags = new VoidObservableCollection<string>();
		public override ObservableCollection<string> Tags { get { return _tags; } }

		private int _updateSequenceNumber = 0;
		public int UpdateSequenceNumber { get { return _updateSequenceNumber; } set { _updateSequenceNumber = value; OnPropertyChanged(); } }

		private readonly EvernoteConfig _config;

		public EvernoteNote(Guid id, EvernoteConfig cfg)
		{
			_id = id;
			_config = cfg;
		}

		public override XElement Serialize()
		{
			var data = new object[]
			{
				new XElement("ID", _id),
				new XElement("Title", _title),
				new XElement("Text", XHelper.ConvertToC80Base64(_text)),
				new XElement("ModificationDate", ModificationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")),
				new XElement("CreationDate", _creationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")),
				new XElement("UpdateSequenceNumber", _updateSequenceNumber),
			};

			var r = new XElement("evernote", data);
			r.SetAttributeValue("plugin", EvernotePlugin.Name);
			r.SetAttributeValue("pluginversion", EvernotePlugin.Version.ToString());

			return r;
		}

		public override void Deserialize(XElement input)
		{
			using (SuppressDirtyChanges())
			{
				_id = XHelper.GetChildValueGUID(input, "ID");
				_title = XHelper.GetChildValueString(input, "Title");
				_text = XHelper.GetChildBase64String(input, "Text");
				_creationDate = XHelper.GetChildValueDateTimeOffset(input, "CreationDate");
				_modificationDate = XHelper.GetChildValueDateTimeOffset(input, "ModificationDate");
				_updateSequenceNumber = XHelper.GetChildValueInt(input, "UpdateSequenceNumber");
			}
		}

		public override string GetUniqueName()
		{
			return "("+_id.ToString("P")+")";
		}

		public override void OnAfterUpload(INote iother)
		{
			var other = (EvernoteNote)iother;

			_updateSequenceNumber = other.UpdateSequenceNumber;
			_modificationDate = other.ModificationDate;
		}

		public override void ApplyUpdatedData(INote iother)
		{
			var other = (EvernoteNote)iother;

			using (SuppressDirtyChanges())
			{
				_modificationDate = other.ModificationDate;
				_updateSequenceNumber = other.UpdateSequenceNumber;
				_tags.Synchronize(other.Tags);
				_title = other.Title;
				_text = other.Text;
			}
		}

		protected override BasicNote CreateClone()
		{
			var n = new EvernoteNote(_id, _config);
			n._title = _title;
			n._text = _text;
			n._creationDate = _creationDate;
			n._modificationDate = _modificationDate;
			n._updateSequenceNumber = _updateSequenceNumber;
			return n;
		}

		public string CreateENML()
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
			builder.AppendLine("<!DOCTYPE en-note SYSTEM \"http://xml.evernote.com/pub/enml2.dtd\">");
			builder.AppendLine("<en-note>");
			foreach (var line in Regex.Split(Text, @"\r?\n"))
			{
				if (line.Trim() == "")
				{
					builder.AppendLine("<div><br /></div>");
				}
				else
				{
					builder.AppendLine("<div>" + new XText(line) + "</div>");
				}
			}
			builder.AppendLine("</en-note>");

			return builder.ToString();
		}

		public void SetTextFromENML(string enml)
		{
			Text = HtmlToPlainText(enml);
		}

		private static string HtmlToPlainText(string html)
		{
			const string tagWhiteSpace = @"(>|$)(\W|\n|\r)+<";//matches one or more (white space or line breaks) between '>' and '<'
			const string stripFormatting = @"<[^>]*(>|$)";//match any character between '<' and '>', even when end tag is missing
			const string lineBreak = @"<(br|BR)\s{0,1}\/{0,1}>";//matches: <br>,<br/>,<br />,<BR>,<BR/>,<BR />
			var lineBreakRegex = new Regex(lineBreak, RegexOptions.Multiline);
			var stripFormattingRegex = new Regex(stripFormatting, RegexOptions.Multiline);
			var tagWhiteSpaceRegex = new Regex(tagWhiteSpace, RegexOptions.Multiline);

			var text = html;
			//Decode html specific characters
			text = System.Net.WebUtility.HtmlDecode(text);
			//Remove tag whitespace/line breaks
			text = tagWhiteSpaceRegex.Replace(text, "><");
			//Replace <br /> with line breaks
			text = lineBreakRegex.Replace(text, Environment.NewLine);
			//Strip formatting
			text = stripFormattingRegex.Replace(text, string.Empty);

			return text;
		}
	}
}
