using System;
using MSHC.WPF.MVVM;

namespace AlephNote.WPF.Util
{
	public class CheckableTag : ObservableObject
	{
		public string Name { get; }

		private bool _checked = false;
		public bool Checked
		{
			get => _checked;
			set { _checked = value; OnPropertyChanged();_lambda?.Invoke(Name, _checked); }
		}

		private readonly Action<string, bool> _lambda;

		public CheckableTag(string txt, Action<string, bool> clickAction)
		{
			Name = txt;
			_lambda = clickAction;
		}
	}
}
