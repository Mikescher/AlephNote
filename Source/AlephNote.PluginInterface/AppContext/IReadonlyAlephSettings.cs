using System;

namespace AlephNote.PluginInterface.AppContext
{
	public interface IReadonlyAlephSettings
	{
		bool AllowAllCharactersInFilename { get; }
		bool AllowAllLettersInFilename { get; }
		Guid ClientID { get; }
		int UsedURLMatchingMode { get; }
	}
}
