using System.Collections.ObjectModel;

namespace AlephNote.PluginInterface.Util
{
	public class VoidObservableCollection<T> : ObservableCollection<T>
	{
		protected override void ClearItems() { }
		protected override void RemoveItem(int index) { }
		protected override void InsertItem(int index, T item) { }
		protected override void SetItem(int index, T item) { }
		protected override void MoveItem(int oldIndex, int newIndex) { }
	}
}
