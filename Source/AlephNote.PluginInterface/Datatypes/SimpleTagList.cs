using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AlephNote.PluginInterface.Datatypes
{
	public class SimpleTagList : TagList
	{
		private ObservableCollection<string> _backend = new ObservableCollection<string>();

		public SimpleTagList()
		{
			_backend.CollectionChanged += (s, e) => CallOnChanged(e);
		}

		public override string this[int index] { get => _backend[index]; set => _backend[index] = value; }

		public override int Count => _backend.Count;

		public override bool IsReadOnly => false;

		public override void Add(string item) => _backend.Add(item);

		public override void Clear() => _backend.Clear();

		public override bool Contains(string item) => _backend.Contains(item);

		public override void CopyTo(string[] array, int arrayIndex) => _backend.CopyTo(array, arrayIndex);

		public override IEnumerator<string> GetEnumerator() => _backend.GetEnumerator();

		public override int IndexOf(string item) => _backend.IndexOf(item);

		public override void Insert(int index, string item) => _backend.Insert(index, item);

		public override bool Remove(string item) => _backend.Remove(item);

		public override void RemoveAt(int index) => _backend.RemoveAt(index);
	
		public override void Move(int oldIndex, int newIndex) => _backend.Move(oldIndex, newIndex);
	}
}
