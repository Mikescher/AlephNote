using CommonNote.PluginInterface;
using System;

namespace CommonNote.Plugins.SimpleNote
{
	public class SimpleNotePlugin : ICommonNotePlugin
	{
		public string GetName()
		{
			return "SimpleNote";
		}

		public Version GetVersion()
		{
			return new Version(0, 0, 0, 1);
		}
	}
}
