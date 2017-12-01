using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlephNote.WPF.Util
{
	public interface IChangeListener
	{
		void OnChanged(string source, int id, object value);
	}
}
