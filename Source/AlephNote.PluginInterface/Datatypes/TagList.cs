using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace AlephNote.PluginInterface.Datatypes
{
	public abstract class TagList : IList<string>, INotifyCollectionChanged
	{
		public abstract IEnumerator<string> GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public abstract void Add(string item);
		public abstract void Clear();
		public abstract bool Contains(string item);
		public abstract void CopyTo(string[] array, int arrayIndex);
		public abstract bool Remove(string item);
		public abstract int Count { get; }
		public abstract bool IsReadOnly { get; }
		public abstract int IndexOf(string item);
		public abstract void Insert(int index, string item);
		public abstract void RemoveAt(int index);
		public abstract string this[int index] { get; set; }
		public abstract void Move(int oldIndex, int newIndex);

		public event EventHandler<NotifyCollectionChangedEventArgs> OnChanged;
		
		protected void CallOnChanged(NotifyCollectionChangedEventArgs e)
		{
			OnChanged?.Invoke(this, e);
		}

		public virtual void CallOnCollectionChanged()
		{
			// Not sure if this is the correct ActionType for "Anything could have changed ??"
			CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}
	}
}
