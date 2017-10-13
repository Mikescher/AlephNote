using System.Diagnostics;
using System.Text;

namespace AlephNote.Common.Extensions
{
	public struct ProcessOutput
	{
		public readonly string Command;
		public readonly int ExitCode;
		public readonly string StdOut;
		public readonly string StdErr;

		public ProcessOutput(string cmd, int ex, string stdout, string stderr)
		{
			Command = cmd;
			ExitCode = ex;
			StdOut = stdout;
			StdErr = stderr;
		}

		public override string ToString() => $"{Command}\n=> {ExitCode}\n\n[stdout]\n{StdOut}\n\n[stderr]\n{StdErr}";
	}

	public static class ProcessHelper
	{
		public static ProcessOutput ProcExecute(string command, string arguments, string workingDirectory = null)
		{
			Process process = new Process
			{
				StartInfo =
				{
					FileName = command,
					Arguments = arguments,
					WorkingDirectory = workingDirectory ?? string.Empty,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true
				}
			};

			StringBuilder builderOut = new StringBuilder();
			StringBuilder builderErr = new StringBuilder();

			process.OutputDataReceived += (sender, args) =>
			{
				if (args.Data == null) return;

				if (builderOut.Length == 0)
					builderOut.Append(args.Data);
				else
					builderOut.Append("\n" + args.Data);
			};

			process.ErrorDataReceived += (sender, args) =>
			{
				if (args.Data == null) return;

				if (builderErr.Length == 0)
					builderErr.Append(args.Data);
				else
					builderErr.Append("\n" + args.Data);
			};

			process.Start();

			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			process.WaitForExit();

			return new ProcessOutput($"{command} {arguments.Replace("\r", "\\r").Replace("\n", "\\n")}", process.ExitCode, builderOut.ToString(), builderErr.ToString());
		}
	}
}
