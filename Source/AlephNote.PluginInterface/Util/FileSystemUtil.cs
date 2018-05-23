using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AlephNote.PluginInterface.Util
{
	public static class FileSystemUtil
	{
		public static IEnumerable<string> EnumerateFilesDeep(string baseFolder, int remainingDepth)
		{
			if (remainingDepth == 0) yield break;

			foreach (var f in Directory.EnumerateFiles(baseFolder)) yield return f;

			foreach (var d in Directory.EnumerateDirectories(baseFolder))
			{
				foreach (var f in EnumerateFilesDeep(d, remainingDepth-1)) yield return f;
			}
		}
		
		public static IEnumerable<string> EnumerateFilesDeep(string baseFolder, int remainingDepth, string[] excludedDirectories)
		{
			if (remainingDepth == 0) yield break;

			foreach (var f in Directory.EnumerateFiles(baseFolder)) yield return f;

			foreach (var d in Directory.EnumerateDirectories(baseFolder))
			{
				if (excludedDirectories.Any(ed => ed == Path.GetFileName(d))) continue;

				foreach (var f in EnumerateFilesDeep(d, remainingDepth-1, excludedDirectories)) yield return f;
			}
		}

		public static void DeleteFileAndFolderIfEmpty(string logsrc, IAlephLogger log, string baseFolder, string file)
		{
			File.Delete(file);

			DeleteFolderIfEmpty(logsrc, log, baseFolder, Path.GetDirectoryName(file));
		}

		public static void DeleteFolderIfEmpty(string logsrc, IAlephLogger log, string baseFolder, string folder)
		{
			var p1 = Path.GetFullPath(baseFolder).TrimEnd(Path.DirectorySeparatorChar).ToLower();
			var p2 = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar).ToLower();
			if (p1 == p2) return;
			if (p1.Count(c => c == Path.DirectorySeparatorChar) >= p2.Count(c => c == Path.DirectorySeparatorChar)) return;

			if (Directory.EnumerateFileSystemEntries(folder).Any()) return;

			log.Debug(logsrc, $"Cleanup empty folder '{p2}' (base = '{p1}')");
			Directory.Delete(folder);
		}

		public static void DeleteFolderIfEmpty(string baseFolder, string folder)
		{
			var p1 = Path.GetFullPath(baseFolder).TrimEnd(Path.DirectorySeparatorChar).ToLower();
			var p2 = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar).ToLower();
			if (p1 == p2) return;
			if (p1.Count(c => c == Path.DirectorySeparatorChar) >= p2.Count(c => c == Path.DirectorySeparatorChar)) return;

			if (Directory.EnumerateFileSystemEntries(folder).Any()) return;

			Directory.Delete(folder);
		}

		public static DirectoryPath GetDirectoryPath(string pBase, string pInfo)
		{
			var sBase = Path.GetFullPath(pBase).TrimEnd(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);
			var sInfo = Path.GetFullPath(pInfo).TrimEnd(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);

			var relative = sInfo.Skip(sBase.Length);

			return DirectoryPath.Create(relative);
		}

		public static string MakePathRelative(string fromPath, string baseDir)
		{
			const string pathSep = "\\";
			string[] p1 = Regex.Split(fromPath, "[\\\\/]").Where(x => x.Length != 0).ToArray();
			string[] p2 = Regex.Split(baseDir, "[\\\\/]").Where(x => x.Length != 0).ToArray();

			int i = 0;
			for (; i < p1.Length && i < p2.Length; i++)
			{
				if (string.Compare(p1[i], p2[i], StringComparison.OrdinalIgnoreCase) != 0) break;
			}

			return string.Join(pathSep, p1.Skip(i));
		}

		public static IEnumerable<string> EnumerateEmptyDirectories(string path, int remainingDepth)
		{
			if (remainingDepth == 0) yield break;

			foreach (var dir in Directory.EnumerateDirectories(path))
			{
				if (Directory.EnumerateFiles(dir).Any()) continue;

				var subdirs = Directory.EnumerateDirectories(dir).ToList();

				foreach (var rec in EnumerateEmptyDirectories(dir, remainingDepth-1))
				{
					subdirs.Remove(rec);
				}

				if (subdirs.Count==0) yield return dir;
			}
		}
	}
}
