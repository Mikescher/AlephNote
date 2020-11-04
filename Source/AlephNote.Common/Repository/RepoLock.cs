using AlephNote.Common.Exceptions;
using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlephNote.Common.Repository
{
    public class RepoLock
    {
        private readonly AlephLogger _logger;
        private readonly string _lockfile;
        private readonly FileStream _stream;

        public RepoLock(AlephLogger al, string fn, FileStream fl)
        {
            _logger   = al;
            _lockfile = fn;
            _stream   = fl;
        }

        public static RepoLock Lock(AlephLogger logger, string pathLocalFolder)
        {
            var filename = Path.Combine(pathLocalFolder, ".lock");

            var content =
                $"[[AlephNote::RepoLock::Lockfile]]\n" +
                $"{{\n" +
                $"    ProcessID     := {Process.GetCurrentProcess().Id}\n" +
                $"    ProcessHandle := {Process.GetCurrentProcess().Handle.ToInt64()}\n" +
                $"    StartTime     := {Process.GetCurrentProcess().StartTime:u}\n" +
                $"    FileName      := {Path.GetFileName(Process.GetCurrentProcess().MainModule?.FileName)}\n" +
                $"    FilePath      := {Process.GetCurrentProcess().MainModule?.FileName}\n" +
                $"    ModuleName    := {Process.GetCurrentProcess().MainModule?.ModuleName}\n" +
                $"    ProcessName   := {Process.GetCurrentProcess().ProcessName}\n" +
                $"}}\n";

            try
            {
                if (File.Exists(filename))
                {
                    try
                    {
                        var oldcontent = File.ReadAllText(filename);
                        logger.Warn("RepoLock", "Old Lockfile found", $"File := {filename}\n\nContent:\n{oldcontent}");
                    }
                    catch (Exception)
                    {
                        logger.Warn("RepoLock", "Old Lockfile found (but could not read)", $"File := {filename}");
                    }
                }

                logger.Debug("RepoLock", "Trying to acquire lock", $"File := {filename}\n\nContent:\n{content}");
                var fs = File.Open(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                var bytes = Encoding.UTF8.GetBytes(content);
                fs.SetLength(0);
                fs.Write(bytes, 0, bytes.Length);
                fs.Flush();
                logger.Debug("RepoLock", "Lock acquired successfully", $"File := {filename}\n\nContent:\n{content}");
                return new RepoLock(logger, filename, fs);
            }
            catch (Exception e1)
            {
                logger.Error("RepoLock", "Acquiring lock failed", $"File := {filename}\n\nException:\n{e1}");

                try
                {
                    var oldcontent = File.ReadAllText(filename);
                    logger.Info("RepoLock", "Old lock file read", $"File := Content:\n{oldcontent}");
                }
                catch (Exception e2)
                {
                    logger.Error("RepoLock", "Cannot read existing lockfile", e2.ToString());
                }

                throw new RepoLockedException($"Could not open Repository '{pathLocalFolder}'.\nThe repository is currently locked by a different process.", e1);
            }
        }

        public void Release()
        {
            _logger.Debug("RepoLock", "Lock released", $"File := {_lockfile}");
            _stream.Close();
            File.Delete(_lockfile);
        }
    }
}
