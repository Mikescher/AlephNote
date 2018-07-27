using System;

namespace AlephNote.WPF.Controls
{
	public class TokenizedTagEventArgs : EventArgs
	{
		public TokenizedTagItem Item { get; set; }

		public TokenizedTagEventArgs(TokenizedTagItem item)
		{
			this.Item = item;
		}
	}
}
