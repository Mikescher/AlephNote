using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AlephNote.PluginInterface.Util;

namespace AlephNote.PluginInterface.Datatypes
{
	public class VoidTagList : TagList
	{
		public override string this[int index] { get { throw new IndexOutOfRangeException();  } set { } }

		public override int Count => 0;

		public override bool IsReadOnly => false;

		public override void Add(string item) { }

		public override void Clear() { }

		public override bool Contains(string item) => false;

		public override void CopyTo(string[] array, int arrayIndex) { }

		public override IEnumerator<string> GetEnumerator() => new ObservableCollection<string>().GetEnumerator();

		public override int IndexOf(string item) => -1;

		public override void Insert(int index, string item) { }

		public override bool Remove(string item) => false;

		public override void RemoveAt(int index) { }
		
		public override void Move(int oldIndex, int newIndex) { }
		
		public override void CallOnCollectionChanged() { }
	}
}
