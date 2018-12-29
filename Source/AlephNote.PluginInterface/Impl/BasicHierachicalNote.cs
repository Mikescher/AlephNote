namespace AlephNote.PluginInterface.Impl
{
	public abstract class BasicHierachicalNote : BasicNoteImpl
	{
		public override void TriggerOnChanged(bool doNotSendChangeEvents)
		{
			if (doNotSendChangeEvents)
			{
				using (SuppressDirtyChanges())
				{
					OnExplicitPropertyChanged("Text");
					OnExplicitPropertyChanged("Title");
					OnExplicitPropertyChanged("Path");
					OnExplicitPropertyChanged("Tags");
					OnExplicitPropertyChanged("ModificationDate");
					OnExplicitPropertyChanged("IsPinned");
					OnExplicitPropertyChanged("IsLocked");
				}
			}
			else
			{
				OnExplicitPropertyChanged("Text");
				OnExplicitPropertyChanged("Title");
				OnExplicitPropertyChanged("Path");
				OnExplicitPropertyChanged("Tags");
				OnExplicitPropertyChanged("ModificationDate");
				OnExplicitPropertyChanged("IsPinned");
				OnExplicitPropertyChanged("IsLocked");
			}
		}
	}
}
