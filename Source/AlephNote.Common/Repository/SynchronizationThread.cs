using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AlephNote.Common.Settings;
using AlephNote.Common.Settings.Types;
using AlephNote.Common.Util;
using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Exceptions;
using MSHC.Lang.Collections;

namespace AlephNote.Common.Repository
{
	public class SynchronizationThread
	{
		private readonly AlephLogger _log = LoggerSingleton.Inst;

		private readonly NoteRepository _repo;
		private readonly List<ISynchronizationFeedback> _listener;
		private readonly ConflictResolutionStrategy _conflictStrategy;
		private readonly bool _noteDownloadEnableMultithreading;
		private readonly int _noteDownloadParallelismLevel;
		private readonly int _noteDownloadParallelismThreshold;
		private readonly bool _noteNewDownloadEnableMultithreading;
		private readonly int _noteNewDownloadParallelismLevel;
		private readonly int _noteNewDownloadParallelismThreshold;
		private readonly bool _noteUploadEnableMultithreading;
		private readonly int _noteUploadParallelismLevel;
		private readonly int _noteUploadParallelismThreshold;

		private int _delay;

		private static readonly object _syncobj = new object();
		private readonly ManualResetEvent _comChannel;
		private Thread _thread;

		private readonly IAlephDispatcher _dispatcher;

		private volatile bool _prioritysync = false;
		private volatile bool _cancel = false;
		private volatile bool _running = false;
		private volatile bool _isSyncing = false;

		public SynchronizationThread(NoteRepository repository, IEnumerable<ISynchronizationFeedback> synclistener, AppSettings settings, IAlephDispatcher disp)
		{
			_repo = repository;
			_listener = synclistener.ToList();

			_conflictStrategy = ConflictResolutionStrategyHelper.ToInterfaceType(settings.ConflictResolution);
			_noteDownloadEnableMultithreading    = repository.SupportsDownloadMultithreading;
			_noteDownloadParallelismLevel        = settings.NoteDownloadParallelismLevel;
			_noteDownloadParallelismThreshold    = settings.NoteDownloadParallelismThreshold;
			_noteNewDownloadEnableMultithreading = repository.SupportsNewDownloadMultithreading;
			_noteNewDownloadParallelismLevel     = settings.NoteNewDownloadParallelismLevel;
			_noteNewDownloadParallelismThreshold = settings.NoteNewDownloadParallelismThreshold;
			_noteUploadEnableMultithreading      = repository.SupportsUploadMultithreading;
			_noteUploadParallelismLevel          = settings.NoteUploadParallelismLevel;
			_noteUploadParallelismThreshold      = settings.NoteUploadParallelismThreshold;

			_dispatcher = disp;
			_comChannel = new ManualResetEvent(false);
		}

		public void Start(int syncdelay)
		{
			if (_running) throw new Exception("SynchronizationThread already running");

			_delay = syncdelay;
			_cancel = false;
			_thread = new Thread(ThreadRun) { Name = "SYNC_THREAD", IsBackground = true };
			_thread.Start();
		}

		private void ThreadRun()
		{
			_running = true;

			for (;;)
			{
				lock (_syncobj)
				{
					_isSyncing = true;
					{
						_comChannel.Reset();
						DoSync();
					}
					_isSyncing = false;
				}

				var tick = Environment.TickCount;
				do
				{
					if (_cancel) { _running = false; _log.Info("Sync", "Thread cancelled"); return; }
					if (_prioritysync) { _prioritysync = false; _log.Info("Sync", "Thread ffwd (priority sync)"); break; }

					_comChannel.WaitOne(10 * 1000); // 10 sec

				} while (Environment.TickCount - tick < _delay);
			}
		}

		private void DoSync()
		{
			_log.Info("Sync", "Starting remote synchronization");

			List<Tuple<string, Exception>> errors = new List<Tuple<string, Exception>>();

			_dispatcher.BeginInvoke(() => { foreach (var l in _listener) l.StartSync(); });

			try
			{
				var data = _repo.GetSyncData();

				List<Tuple<INote, INote>> allNotes = new List<Tuple<INote, INote>>(); // <real, clone>
				List<INote> notesToDelete = new List<INote>();
				_dispatcher.Invoke(() =>
				{
					allNotes = _repo.Notes.Select(p => Tuple.Create(p, p.Clone())).ToList();
					notesToDelete = _repo.LocalDeletedNotes.ToList();
				});

				_log.Info(
					"Sync",
					$"Found {allNotes.Count} alive notes and {notesToDelete.Count} deleted notes",
					$"Alive:\n{string.Join("\n", allNotes.Select(n => $"{n.Item2.UniqueName}    {n.Item2.Title}"))}\n\n\n" +
					$"Deleted:\n{string.Join("\n", notesToDelete.Select(n => $"{n.UniqueName}    {n.Title}"))}");

				_repo.Connection.StartSync(data, allNotes.Select(p => p.Item2).ToList(), notesToDelete);
				{
					// plugin says 'upload'
					var notesToUpload     = allNotes.Where(p => _repo.Connection.NeedsUpload(p.Item2)).ToList();
					// plugin says 'download'
					var notesToDownload   = allNotes.Where(p => _repo.Connection.NeedsDownload(p.Item2)).ToList();
					// we think 'upload', but provider doesn't say so
					var notesToResetDirty = allNotes.Where(p => !p.Item2.IsRemoteSaved && !notesToUpload.Contains(p)).ToList();

					_log.Info(
						"Sync",
						$"Found {notesToUpload.Count} notes for upload and {notesToDownload.Count} notes for download",
						$"Upload:\n{string.Join("\n", notesToUpload.Select(n => $"{n.Item2.UniqueName}    {n.Item2.Title}"))}\n\n\n" +
						$"Download:\n{string.Join("\n", notesToDownload.Select(n => $"{n.Item2.UniqueName}    {n.Item2.Title}"))}");

					UploadNotes(notesToUpload, notesToResetDirty, errors);

					DownloadNotes(notesToDownload, errors);

					DeleteNotes(notesToDelete, errors);

					DownloadNewNotes(allNotes, errors);
				}
				_repo.Connection.FinishSync();

				_repo.WriteSyncData(data);
			}
			catch (RestException e)
			{
				errors.Add(Tuple.Create<string, Exception>("Execption while syncing notes: " + e.ShortMessage, e));
			}
			catch (Exception e)
			{
				errors.Add(Tuple.Create("Execption while syncing notes: " + e.Message, e));
			}

			if (errors.Any())
			{
				_dispatcher.BeginInvoke(() => { foreach (var l in _listener) l.SyncError(errors); });
			}
			else
			{
				_dispatcher.BeginInvoke(() => { foreach (var l in _listener) l.SyncSuccess(DateTimeOffset.Now); });
			}

			_log.Info("Sync", "Finished remote synchronization");
		}

		private void UploadNotes(IReadOnlyCollection<Tuple<INote, INote>> notesToUpload, IEnumerable<Tuple<INote, INote>> notesToResetRemoteDirty, ICollection<Tuple<string, Exception>> errors)
		{
			ExecuteInParallel(
				_log,
				"UploadNotes",
				_noteUploadEnableMultithreading,
				notesToUpload,
				_noteUploadParallelismLevel,
				_noteUploadParallelismThreshold,
				(e, notetuple) =>
				{
					var message = string.Format("Could not upload note '{2}' ({0}) cause of {1}", notetuple.Item2.UniqueName, e.Message, notetuple.Item2.Title);

					_log.Error("Sync", message, e);
					errors.Add(Tuple.Create(message, e));
					return true;
				},
				notetuple =>
				{
					var realnote = notetuple.Item1;
					var clonenote = notetuple.Item2;

					_log.Info("Sync", string.Format("Upload note {0}", clonenote.UniqueName));

					if (!realnote.IsLocalSaved)
					{
						_dispatcher.Invoke(() =>
						{
							if (!realnote.IsLocalSaved) _repo.SaveNote(realnote);
						});
					}

					var result = _repo.Connection.UploadNoteToRemote(ref clonenote, out var conflictnote, _conflictStrategy);

					switch (result)
					{
						case RemoteUploadResult.UpToDate:
						case RemoteUploadResult.Uploaded:
							_dispatcher.Invoke(() =>
							{
								if (realnote.IsLocalSaved)
								{
									realnote.OnAfterUpload(clonenote);
									realnote.ResetRemoteDirty("Note was successfully uploaded (no conflict)");
									_repo.SaveNote(realnote);
								}
							});
							break;

						case RemoteUploadResult.Merged:
							_dispatcher.Invoke(() =>
							{
								realnote.ApplyUpdatedData(clonenote);
								realnote.TriggerOnChanged(true);
								realnote.SetLocalDirty("Note was uploaded and a merge has happened");
								realnote.ResetRemoteDirty("Note was successfully uploaded (auto-merge)");
							});
							break;

						case RemoteUploadResult.Conflict:
							_log.Warn("Sync", "Uploading note " + clonenote.UniqueName + " resulted in conflict");
							ResolveUploadConflict(realnote, clonenote, conflictnote);
							break;

						default:
							throw new ArgumentOutOfRangeException();
					}
				});

			foreach (var notetuple in notesToResetRemoteDirty)
			{
				var realnote = notetuple.Item1;
				var clonenote = notetuple.Item2;

				_log.Info("Sync", string.Format("Reset remote dirty of note {0} (no upload needed)", clonenote.UniqueName));

				try
				{
					if (!realnote.IsLocalSaved)
					{
						_dispatcher.Invoke(() =>
						{
							if (!realnote.IsLocalSaved) _repo.SaveNote(realnote);
						});
					}

					_dispatcher.Invoke(() =>
					{
						if (realnote.IsLocalSaved)
						{
							realnote.ResetRemoteDirty("Reset remote dirty - was marked for upload but plugin says no upload is needed");
							_repo.SaveNote(realnote);
						}
					});
				}
				catch (Exception e)
				{
					var message = string.Format("Could not reset remote dirty note '{2}' ({0}) cause of {1}", clonenote.UniqueName, e.Message, clonenote.Title);

					_log.Error("Sync", message, e);
					errors.Add(Tuple.Create(message, e));
				}
			}
		}

		/// <param name="realnote">The real note in the repository (owned by UI Thread)</param>
		/// <param name="clonenote">The new note data</param>
		/// <param name="conflictnote">The conflicting note</param>
		private void ResolveUploadConflict(INote realnote, INote clonenote, INote conflictnote)
		{
			switch (_conflictStrategy)
			{
				case ConflictResolutionStrategy.UseClientVersion:
					_dispatcher.Invoke(() =>
					{
						if (realnote.IsLocalSaved)
						{
							realnote.OnAfterUpload(clonenote);
							realnote.ResetRemoteDirty("Upload conflict was solved by [UseClientVersion]");
							_repo.SaveNote(realnote);

							_log.Warn("Sync", "Resolve conflict: UseClientVersion");
						}
						else
						{
							_log.Warn("Sync", "Resolve conflict: UseClientVersion (do nothing cause note changed locally)");
						}
					});
					break;
				case ConflictResolutionStrategy.UseServerVersion:
					_dispatcher.Invoke(() =>
					{
						realnote.ApplyUpdatedData(clonenote);
						realnote.TriggerOnChanged(true);
						realnote.SetLocalDirty("Upload conflict was solved by [UseServerVersion]");
						realnote.ResetRemoteDirty("Upload conflict was solved by [UseServerVersion]");
						_repo.SaveNote(realnote);

						_log.Warn("Sync", "Resolve conflict: UseServerVersion");
					});
					break;
				case ConflictResolutionStrategy.UseClientCreateConflictFile:
					_dispatcher.Invoke(() =>
					{
						if (realnote.IsLocalSaved)
						{
							realnote.OnAfterUpload(clonenote);
							realnote.ResetRemoteDirty("Upload conflict was solved by [UseClientCreateConflictFile]");
							_repo.SaveNote(realnote);
						}
						else
						{
							_log.Warn("Sync", "Resolve conflict: UseClientCreateConflictFile (do not [OnAfterUpload] cause note changed locally)");
						}

						var conflict = _repo.CreateNewNote(conflictnote.Path);
						conflict.Title = string.Format("{0}_conflict-{1:yyyy-MM-dd_HH:mm:ss}", conflictnote.Title, DateTime.Now);
						conflict.Text = conflictnote.Text;
						conflict.Tags.Synchronize(conflictnote.Tags);
						conflict.IsConflictNote = true;
						_repo.SaveNote(conflict);

						_log.Warn("Sync", "Resolve conflict: UseClientCreateConflictFile (conflictnote: " + conflict.UniqueName + ")");
					});
					break;
				case ConflictResolutionStrategy.UseServerCreateConflictFile:
					_dispatcher.Invoke(() =>
					{
						realnote.ApplyUpdatedData(clonenote);
						realnote.TriggerOnChanged(true);
						realnote.SetLocalDirty("Upload conflict was solved by [UseServerCreateConflictFile]");
						realnote.ResetRemoteDirty("Upload conflict was solved by [UseServerCreateConflictFile]");
						_repo.SaveNote(realnote);

						var conflict = _repo.CreateNewNote(conflictnote.Path);
						conflict.Title = string.Format("{0}_conflict-{1:yyyy-MM-dd_HH:mm:ss}", conflictnote.Title, DateTime.Now);
						conflict.Text = conflictnote.Text;
						conflict.Tags.Synchronize(conflictnote.Tags);
						conflict.IsConflictNote = true;
						_repo.SaveNote(conflict);

						_log.Warn("Sync", "Resolve conflict: UseServerCreateConflictFile (conflictnote: " + conflict.UniqueName + ")");
					});
					break;
				case ConflictResolutionStrategy.ManualMerge:
					_dispatcher.Invoke(() =>
					{
						if (realnote.IsLocalSaved)
						{
							realnote.OnAfterUpload(clonenote);
							realnote.ResetRemoteDirty("Upload conflict was solved by [ManualMerge]");
							_repo.SaveNote(realnote);
						}
						else
						{
							_log.Warn("Sync", "Resolve conflict: ManualMerge (do not [OnAfterUpload] cause note changed locally)");
						}

						var txt0 = clonenote.Text;
						var txt1 = conflictnote.Text;

						var ttl0 = clonenote.Title;
						var ttl1 = conflictnote.Title;

						var tgs0 = clonenote.Tags.OrderBy(p => p).ToList();
						var tgs1 = conflictnote.Tags.OrderBy(p => p).ToList();

						var ndp0 = clonenote.Path;
						var ndp1 = conflictnote.Path;

						if (txt0 != txt1 || ttl0 != ttl1 || !tgs0.CollectionEquals(tgs1) || ndp0 != ndp1)
						{
							_log.Info(
								"Sync",
								"Resolve conflict: ManualMerge :: Show dialog",
								$"======== Title 1 ========\n{ttl0}\n\n======== Title 2 ========\n{ttl1}\n\n" +
								$"======== Text 1 ========\n{txt0}\n\n======== Text 2 ========\n{txt1}\n\n" +
								$"======== Tags 1 ========\n{string.Join(" | ", tgs0)}\n\n======== Tags 2 ========\n{string.Join(" | ", tgs1)}\n\n");

							_dispatcher.BeginInvoke(() =>
							{
								_repo.ShowConflictResolutionDialog(clonenote.UniqueName, txt0, ttl0, tgs0, ndp0, txt1, ttl1, tgs1, ndp1);
							});
						}
						else
						{
							_log.Info("Sync", "Resolve conflict: ManualMerge not executed cause conflict==real");
						}
					});
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void DownloadNotes(List<Tuple<INote, INote>> notesToDownload, ICollection<Tuple<string, Exception>> errors)
		{
			ExecuteInParallel(
				_log,
				"DownloadNotes",
				_noteDownloadEnableMultithreading,
				notesToDownload,
				_noteDownloadParallelismLevel,
				_noteDownloadParallelismThreshold,
				(e, notetuple) =>
				{
					var message = string.Format("Could not synchronize note '{2}' ({0}) cause of {1}", notetuple.Item2.UniqueName, e.Message, notetuple.Item2.Title);
					_log.Error("Sync", message, e);
					errors.Add(Tuple.Create(message, e));
					return true;
				},
				notetuple =>
				{
					var realnote = notetuple.Item1;
					var clonenote = notetuple.Item2;

					_log.Info("Sync", string.Format("Download note {0}", clonenote.UniqueName));
					if (!realnote.IsLocalSaved)
					{
						_log.Warn("Sync", "Downloading note skipped (unsaved changes)");
						return;
					}

					var result = _repo.Connection.UpdateNoteFromRemote(clonenote);

					switch (result)
					{
						case RemoteDownloadResult.UpToDate:
							_log.Info("Sync", "Downloading note -> UpToDate");
							if (realnote.ModificationDate != clonenote.ModificationDate)
							{
								_log.Info("Sync", "Downloading note -> UpToDate (but update local mdate)");
								_dispatcher.Invoke(() =>
								{
									if (realnote.IsLocalSaved)
									{
										// Even when up to date - perhaps local mod date is wrong ...
										realnote.ModificationDate = clonenote.ModificationDate;
										realnote.ResetRemoteDirty("Note was downloaded from remote (no changes - UpToDate)");
									}
								});
							}

							break;

						case RemoteDownloadResult.Updated:
							_log.Info("Sync", "Downloading note -> Updated");
							_dispatcher.Invoke(() =>
							{
								if (realnote.IsLocalSaved)
								{
									realnote.ApplyUpdatedData(clonenote);
									realnote.TriggerOnChanged(true);
									realnote.SetLocalDirty("Note was downloaded from remote (local note was updated)");
									realnote.ResetRemoteDirty("Note was downloaded from remote (local note was updated)");
									_repo.SaveNote(realnote);
								}
								else
								{
									_log.Info("Sync", "Downloading (->Updated) skipped (unsaved local changes)");
								}
							});
							break;

						case RemoteDownloadResult.DeletedOnRemote:
							_log.Info("Sync", "Downloading note -> DeletedOnRemote");
							_dispatcher.Invoke(() =>
							{
								if (realnote.IsLocalSaved)
								{
									_repo.DeleteNote(realnote, false);
								}
								else
								{
									_log.Info("Sync", "Downloading (->DeletedOnRemote) skipped (unsaved local changes)");
								}
							});
							break;

						default:
							throw new ArgumentOutOfRangeException();
					}
				});
		}

		private void DeleteNotes(List<INote> notesToDelete, ICollection<Tuple<string, Exception>> errors)
		{
			foreach (var xnote in notesToDelete)
			{
				var note = xnote;

				_log.Info("Sync", $"Delete note {note.UniqueName}");

				try
				{
					_repo.Connection.DeleteNote(note);
					_dispatcher.Invoke(() => _repo.LocalDeletedNotes.Remove(note));
				}
				catch (Exception e)
				{
					var message = $"Could not delete note {note.Title} ({note.UniqueName}) on remote cause of {e.Message}";
					_log.Error("Sync", message, e);
					errors.Add(Tuple.Create(message, e));
				}
			}
		}

		private void DownloadNewNotes(IEnumerable<Tuple<INote, INote>> allNotes, ICollection<Tuple<string, Exception>> errors)
		{
			var missing = _repo.Connection.ListMissingNotes(allNotes.Select(p => p.Item2).ToList());
			
			ExecuteInParallel(
				_log,
				"DownloadNewNotes",
				_noteNewDownloadEnableMultithreading,
				missing,
				_noteNewDownloadParallelismLevel,
				_noteNewDownloadParallelismThreshold,
				(e, xnoteid) =>
				{
					var message = $"Could not download new note '{xnoteid}' on remote cause of {e.Message}";
					_log.Error("Sync", message, e);
					errors.Add(Tuple.Create(message, e));
					return true;
				},
				xnoteid =>
				{
					var note = _repo.Connection.DownloadNote(xnoteid, out var isnewnote);
					if (isnewnote)
					{
						note.SetLocalDirty("New note from remote");
						note.ResetRemoteDirty("New note from remote");
						_dispatcher.Invoke(() => _repo.AddNote(note, false));
					}
					else
					{
						_log.Warn("Sync", $"Download new note {{id:'{xnoteid}'}} returned false");
					}
				});
		}

		public void SyncNowAsync()
		{
			_log.Info("Sync", "Requesting priorty sync");
			RequestPrioritySync();
		}

		public void StopAsync()
		{
			using (_dispatcher.EnableCustomDispatcher())
			{
				_log.Info("Sync", "Requesting stop");

				if (_isSyncing)
				{
					_log.Info("Sync", "Requesting stop (early exit due to isSyncing)");

					RequestCancel();
					WaitForSyncStop(60);

					return;
				}

				_prioritysync = false;
				RequestCancel();
				WaitForStopped(45);
			}
		}

		public void SyncNowAndStopAsync()
		{
			using (_dispatcher.EnableCustomDispatcher())
			{
				_log.Info("Sync", "Requesting sync&stop");

				if (_isSyncing)
				{
					_log.Info("Sync", "Requesting sync&stop (early exit due to isSyncing)");

					RequestCancel();
					WaitForSyncStop(60);

					return;
				}

				RequestPrioritySync();
				WaitForStopped(60);
			}
		}

		private void WaitForStopped(int seconds)
		{
			var startWait1 = Environment.TickCount;
			for (;;)
			{
				if (!_running)
				{
					_log.Info("Sync", "Waiting for SyncThread stopping :: finished (running=false)");
					return;
				}

				if (_isSyncing)
				{
					_log.Info("Sync", "Waiting for SyncThread stopping :: currently syncing (isSyncing=true)");

					RequestCancel();
					WaitForSyncStop(30);

					_log.Info("Sync", "Waiting for SyncThread stopping :: syncing finished (isSyncing=true)");

					RequestCancel();
				}

				SleepDoEvents(10);

				if (Environment.TickCount - startWait1 > seconds * 1000)
				{
					_log.Error("Sync", "Requesting sync&stop failed after timeout (waiting on prioritysync)");
					throw new Exception("Background thread timeout after " + seconds + "sec");
				}
			}
		}

		private void WaitForSyncStop(int seconds)
		{
			var startWait2 = Environment.TickCount;
			for (;;)
			{
				if (!_isSyncing)
				{
					_log.Info("Sync", "Requesting sync&stop finished (isSyncing=false)");
					return;
				}

				if (!_running)
				{
					_log.Info("Sync", "Requesting sync&stop finished (running=false)");
					return;
				}

				SleepDoEvents(10);

				if (Environment.TickCount - startWait2 > seconds * 1000)
				{
					_log.Error("Sync", "Requesting sync&stop failed after timeout (waiting on isSyncing)");
					throw new Exception("Background thread timeout after " + seconds + "sec");
				}
			}
		}

		private void RequestCancel()
		{
			_cancel = true;
			_comChannel.Set();
		}

		private void RequestPrioritySync()
		{
			_prioritysync = true;
			_comChannel.Set();
		}

		public void Kill()
		{
			_cancel = true;
			//_thread?.Abort();
		}

		private void SleepDoEvents(int sleep)
		{
			Thread.Sleep(sleep);
			_dispatcher.Work();
		}

		public static void ExecuteInParallel<T>(AlephLogger log, string taskname, bool enableParallelism, IReadOnlyCollection<T> data, int level, int threshold, Func<Exception, T, bool> error, Action<T> method)
		{
			if (data.Count == 0)
			{
				log.Debug("Sync",
					$"Skip executing {{{taskname}}}",
					$"Multithreading enabled := {enableParallelism}\n" +
					$"Datasize               := {data.Count}\n" +
					$"Threadcount            := {level}\n" +
					$"Threshold              := {threshold}\n");
			}
			else if (data.Count < threshold || level <= 1 || !enableParallelism)
			{
				log.Debug("Sync",
					$"Execute {{{taskname}}} in sequence",
					$"Multithreading enabled := {enableParallelism}\n" +
					$"Datasize               := {data.Count}\n" +
					$"Threadcount            := {level}\n" +
					$"Threshold              := {threshold}\n");

				foreach (var datum in data)
				{
					try
					{
						method.Invoke(datum);
					}
					catch (Exception e)
					{
						error.Invoke(e, datum);
					}
				}
			}
			else
			{
				log.Debug("Sync",
					$"Execute {{{taskname}}} in parallel",
					$"Multithreading enabled := {enableParallelism}\n" +
					$"Datasize               := {data.Count}\n" +
					$"Threadcount            := {level}\n" +
					$"Threshold              := {threshold}\n");

				var work = new ConcurrentQueue<T>();
				foreach (var datum in data) work.Enqueue(datum);

				var tasks = new List<Task>(level);
				for (var i = 0; i < level; i++)
					tasks.Add(Task.Factory.StartNew(() =>
					{
						for (;;)
						{
							if (!work.TryDequeue(out var datum)) return;
							try
							{
								method(datum);
							}
							catch (Exception e)
							{
								var cont = error(e, datum);
								if (!cont)
								{
									// Abort
									while(work.TryDequeue(out _));
									return;
								}
							}
						}
					}));
				
				Task.WaitAll(tasks.ToArray());
			}
		}
	}
}