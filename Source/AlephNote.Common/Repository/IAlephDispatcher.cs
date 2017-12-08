using System;

namespace AlephNote.Common.Repository
{
	public interface IAlephDispatcher
	{
		IDisposable EnableCustomDispatcher();

		void BeginInvoke(Action a);
		void Invoke(Action a);

		void Work();
	}
}
