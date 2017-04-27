using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
