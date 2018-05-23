using AlephNote.Common.MVVM;
using System.Collections.Generic;

namespace AlephNote.WPF.Windows
{
	class ConflictWindowViewmodel : ObservableObject
	{
		private string _text1 = "";
		public string Text1 { get { return _text1; } set { _text1 = value; OnPropertyChanged(); } }
		
		private string _text2 = "";
		public string Text2 { get { return _text2; } set { _text2 = value; OnPropertyChanged(); } }

		private string _title1 = "";
		public string Title1 { get { return _title1; } set { _title1 = value; OnPropertyChanged(); } }
		
		private string _title2 = "";
		public string Title2 { get { return _title2; } set { _title2 = value; OnPropertyChanged(); } }

		private List<string> _tags1 = new List<string>();
		public List<string> Tags1 { get { return _tags1; } set { _tags1 = value; OnPropertyChanged(); } }
		
		private List<string> _tags2 = new List<string>();
		public List<string> Tags2 { get { return _tags2; } set { _tags2 = value; OnPropertyChanged(); } }

		private string _path1 = "";
		public string Path1 { get { return _path1; } set { _path1 = value; OnPropertyChanged(); } }
		
		private string _path2 = "";
		public string Path2 { get { return _path2; } set { _path2 = value; OnPropertyChanged(); } }
	}
}
