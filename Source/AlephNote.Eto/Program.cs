using System;

namespace AlephNote
{
	class Program
	{
		[STAThread]
		static void Main()
		{
			new Eto.Forms.Application().Run(new MainForm());
		}
	}
}
