using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AlephNote.PluginInterface.Util
{
	public static class ANFileSystemUtil
	{

		public static void DeleteFileAndFolderIfEmpty(string logsrc, AlephLogger log, string baseFolder, string file)
		{
			var fi = new FileInfo(file);
			if (fi.Exists && fi.IsReadOnly) fi.IsReadOnly = false;

			File.Delete(file);

			DeleteFolderIfEmpty(logsrc, log, baseFolder, Path.GetDirectoryName(file));
		}

		public static void DeleteFolderIfEmpty(string logsrc, AlephLogger log, string baseFolder, string folder)
		{
			var p1 = Path.GetFullPath(baseFolder).TrimEnd(Path.DirectorySeparatorChar).ToLower();
			var p2 = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar).ToLower();
			if (p1 == p2) return;
			if (p1.Count(c => c == Path.DirectorySeparatorChar) >= p2.Count(c => c == Path.DirectorySeparatorChar)) return;

			if (Directory.EnumerateFileSystemEntries(folder).Any()) return;

			log.Debug(logsrc, $"Cleanup empty folder '{p2}' (base = '{p1}')");
			Directory.Delete(folder);
		}
		
		public static DirectoryPath GetDirectoryPath(string pBase, string pInfo)
		{
			var sBase = Path.GetFullPath(pBase).TrimEnd(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);
			var sInfo = Path.GetFullPath(pInfo).TrimEnd(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);

			var relative = sInfo.Skip(sBase.Length);

			return DirectoryPath.Create(relative);
		}

		public static void DeleteDirectoryWithRetry(AlephLogger logger, string path)
		{
			for (int i = 0; i < 5; i++)
			{
				try
				{
					Directory.Delete(path);
					return;
				}
				catch (IOException e)
				{
					logger.Debug("DeleteDirectoryWithRetry", "Retry Directory delete", "Retry directory delete, exception thrown:\r\n"+e);
					Thread.Sleep(5);
				}
			}
			
			Directory.Delete(path); // Do it again and throw Exception if it fails
		}

	}
}
