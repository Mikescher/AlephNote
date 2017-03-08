using System;

namespace AlephNote.Common.MVVM
{
	public interface IDispatcher
	{
		void BeginInvoke(Action a);
		void Invoke(Action a);
	}
}
