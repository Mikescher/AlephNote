using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace AlephNote.Common.MVVM
{
	public class ObservableCollectionNoReset<T> : ObservableCollection<T>
	{
		// Some CollectionChanged listeners don't support range actions.
		public Boolean RangeActionsSupported { get; set; }

		protected override void ClearItems()
		{
			if (RangeActionsSupported)
			{
				List<T> removed = new List<T>(this);
				base.ClearItems();
				base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed));
			}
			else
			{
				while (Count > 0) RemoveAt(Count - 1);
			}
		}

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if (e.Action != NotifyCollectionChangedAction.Reset)
				base.OnCollectionChanged(e);
		}

		public ObservableCollectionNoReset(Boolean rangeActionsSupported = false)
		{
			RangeActionsSupported = rangeActionsSupported;
		}
	}
}
