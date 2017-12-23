using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace AlephNote.PluginInterface.Objects
{
	public class ObservableCollectionWithClearEvents<T> : ObservableCollection<T>
	{
		public ObservableCollectionWithClearEvents() : base() { }

		public ObservableCollectionWithClearEvents(List<T> collection) : base(collection) { }

		public ObservableCollectionWithClearEvents(IEnumerable<T> collection) : base(collection) { }

		new public void Clear()
		{
			lock (((ICollection)Items).SyncRoot)
			{
				for (int i = this.Items.Count - 1; i >= 0; i--)
				{
					var item = Items[i];

					Items.RemoveAt(i);
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, i));
				}
			}
		}
	}
}
