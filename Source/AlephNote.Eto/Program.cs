using System;

namespace AlephNote
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			new Eto.Forms.Application().Run(new MainForm());
		}
	}
}
