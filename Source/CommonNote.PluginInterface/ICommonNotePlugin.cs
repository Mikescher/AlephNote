
using System;

namespace CommonNote.PluginInterface
{
	public interface ICommonNotePlugin
	{
		string GetName();
		Version GetVersion();
	}
}
