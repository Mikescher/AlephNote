namespace AlephNote.WPF.Util
{
	public interface IChangeListener
	{
		void OnChanged(string source, int id, object value);
	}
}
