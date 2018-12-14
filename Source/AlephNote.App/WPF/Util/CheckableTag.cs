using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlephNote.WPF.Windows;
using MSHC.WPF.MVVM;

namespace AlephNote.WPF.Util
{
	public class CheckableTag : ObservableObject
	{
		public List<CheckableTag> TagGroup = new List<CheckableTag>();

		public string Name {get; set;}

		private bool _checked = false;
		public bool Checked
		{
			get => _checked;
			set { _checked = value; OnPropertyChanged(); UpdateSearchString(); }
		}

		private MainWindowViewmodel _vm;

		public CheckableTag(string txt, MainWindowViewmodel vm)
		{
			Name = txt;
			_vm = vm;
		}

		public void UpdateSearchString()
		{
			_vm.SearchText = string.Join(" ", TagGroup.Where(t => t.Checked).Select(p => "["+p.Name.Replace("\\", "\\\\").Replace("[", "\\[").Replace("]", "\\]") + "]"));
		}
	}
}
