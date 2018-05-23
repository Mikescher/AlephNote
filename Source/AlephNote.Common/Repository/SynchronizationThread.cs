using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AlephNote.Common.Settings.Types;
using AlephNote.Common.Util;
using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Exceptions;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Common.Repository
{
	class SynchronizationThread
	{
		private readonly IAlephLogger _log = LoggerSingleton.Inst;

		private readonly NoteRepository repo;
		private readonly List<ISynchronizationFeedback> listener;
		private readonly ConflictResolutionStrategy conflictStrategy;
		private int delay;

		private static readonly object _syncobj = new object();
		private readonly ManualResetEvent _comChannel;
		private Thread thread;

		private readonly IAlephDispatcher dispatcher;

		private volatile bool prioritysync = false;
		private volatile bool cancel = false;
		private volatile bool running = false;
		private volatile bool isSyncing = false;


		public SynchronizationThread(NoteRepository repository, ISynchronizationFeedback[] synclistener, ConflictResolutionStrategyConfig strat, IAlephDispatcher disp)
		{
			repo = repository;
			listener = synclistener.ToList();
			conflictStrategy = ConflictResolutionStrategyHelper.ToInterfaceType(strat);
			dispatcher = disp;
			_comChannel = new ManualResetEvent(false);
		}

		public void Start(int syncdelay)
		{
			if (running) throw new Exception("SynchronizationThread already running");

			delay = syncdelay;
			cancel = false;
			thread = new Thread(ThreadRun) { Name = "SYNC_THREAD", IsBackground = true };
			thread.Start();
		}

		private void ThreadRun()
		{
			running = true;

			for (; ; )
			{
				lock (_syncobj)
				{
					isSyncing = true;
					{
						_comChannel.Reset();
						DoSync();
					}
					isSyncing = false;
				}

				var tick = Environment.TickCount;
				do
				{
					if (cancel) { running = false; _log.Info("Sync", "Thread cancelled"); return; }
					if (prioritysync) { prioritysync = false; _log.Info("Sync", "Thread ffwd (priority sync)"); break; }

					_comChannel.WaitOne(10 * 1000); // 10 sec

				} while (Environment.TickCount - tick < delay);
			}
		}

		private void DoSync()
		{
			_log.Info("Sync", "Starting remote synchronization");

			List<Tuple<string, Exception>> errors = new List<Tuple<string, Exception>>();

			dispatcher.BeginInvoke(() => { foreach (var l in listener) l.StartSync(); });

			try
			{
				var data = repo.GetSyncData();

				List<Tuple<INote, INote>> allNotes = new List<Tuple<INote, INote>>(); // <real, clone>
				List<INote> notesToDelete = new List<INote>();
				dispatcher.Invoke(() =>
				{
					allNotes = repo.Notes.Select(p => Tuple.Create(p, p.Clone())).ToList();
					notesToDelete = repo.LocalDeletedNotes.ToList();
				});

				_log.Info("Sync", string.Format("Found {0} alive notes and {1} deleted notes", allNotes.Count, notesToDelete.Count));

				repo.Connection.StartSync(data, allNotes.Select(p => p.Item2).ToList(), notesToDelete);
				{
					// plugin says 'upload'
					var notesToUpload     = allNotes.Where(p => repo.Connection.NeedsUpload(p.Item2)).ToList();
					// plugin says 'download'
					var notesToDownload   = allNotes.Where(p => repo.Connection.NeedsDownload(p.Item2)).ToList();
					// we think 'upload', but provider doesn't say so
					var notesToResetDirty = allNotes.Where(p => !p.Item2.IsRemoteSaved && !notesToUpload.Contains(p)).ToList();

					_log.Info("Sync", string.Format("Found {0} notes for upload and {1} notes for download", notesToUpload.Count, notesToDownload.Count));

					UploadNotes(notesToUpload, notesToResetDirty, ref errors);

					DownloadNotes(notesToDownload, ref errors);

					DeleteNotes(notesToDelete, ref errors);

					DownloadNewNotes(allNotes, ref errors);
				}
				repo.Connection.FinishSync();

				repo.WriteSyncData(data);
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
				dispatcher.BeginInvoke(() => { foreach (var l in listener) l.SyncError(errors); });
			}
			else
			{
				dispatcher.BeginInvoke(() => { foreach (var l in listener) l.SyncSuccess(DateTimeOffset.Now); });
			}

			_log.Info("Sync", "Finished remote synchronization");
		}

		private void UploadNotes(List<Tuple<INote, INote>> notesToUpload, List<Tuple<INote, INote>> notesToResetRemoteDirty, ref List<Tuple<string, Exception>> errors)
		{
			foreach (var notetuple in notesToUpload)
			{
				var realnote = notetuple.Item1;
				var clonenote = notetuple.Item2;

				_log.Info("Sync", string.Format("Upload note {0}", clonenote.UniqueName));

				try
				{
					if (!clonenote.IsLocalSaved)
					{
						dispatcher.Invoke(() =>
						{
							if (!realnote.IsLocalSaved) repo.SaveNote(realnote);
						});
					}

					var result = repo.Connection.UploadNoteToRemote(ref clonenote, out var conflictnote, conflictStrategy);

					switch (result)
					{
						case RemoteUploadResult.UpToDate:
						case RemoteUploadResult.Uploaded:
							dispatcher.Invoke(() =>
							{
								if (realnote.IsLocalSaved)
								{
									realnote.OnAfterUpload(clonenote);
									realnote.ResetRemoteDirty();
									repo.SaveNote(realnote);
								}
							});
							break;

						case RemoteUploadResult.Merged:
							dispatcher.Invoke(() =>
							{
								realnote.ApplyUpdatedData(clonenote);
								realnote.TriggerOnChanged(true);
								realnote.SetLocalDirty();
								realnote.ResetRemoteDirty();
							});
							break;

						case RemoteUploadResult.Conflict:
							_log.Warn("Sync", "Uploading note " + clonenote.UniqueName+ " resulted in conflict");
							ResolveUploadConflict(realnote, clonenote, conflictnote);
							break;

						default:
							throw new ArgumentOutOfRangeException();
					}

				}
				catch (Exception e)
				{
					var message = string.Format("Could not upload note '{2}' ({0}) cause of {1}", clonenote.UniqueName, e.Message, clonenote.Title);

					_log.Error("Sync", message, e);
					errors.Add(Tuple.Create(message, e));
				}
			}


			foreach (var notetuple in notesToResetRemoteDirty)
			{
				var realnote = notetuple.Item1;
				var clonenote = notetuple.Item2;

				_log.Info("Sync", string.Format("Reset remote dirty of note {0} (no upload needed)", clonenote.UniqueName));

				try
				{
					if (!clonenote.IsLocalSaved)
					{
						dispatcher.Invoke(() =>
						{
							if (!realnote.IsLocalSaved) repo.SaveNote(realnote);
						});
					}

					dispatcher.Invoke(() =>
					{
						if (realnote.IsLocalSaved)
						{
							realnote.ResetRemoteDirty();
							repo.SaveNote(realnote);
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
			switch (conflictStrategy)
			{
				case ConflictResolutionStrategy.UseClientVersion:
					dispatcher.Invoke(() =>
					{
						if (realnote.IsLocalSaved)
						{
							realnote.OnAfterUpload(clonenote);
							realnote.ResetRemoteDirty();
							repo.SaveNote(realnote);

							_log.Warn("Sync", "Resolve conflict: UseClientVersion");
						}
						else
						{
							_log.Warn("Sync", "Resolve conflict: UseClientVersion (do nothing cause note changed locally)");
						}
					});
					break;
				case ConflictResolutionStrategy.UseServerVersion:
					dispatcher.Invoke(() =>
					{
						realnote.ApplyUpdatedData(clonenote);
						realnote.TriggerOnChanged(true);
						realnote.SetLocalDirty();
						realnote.ResetRemoteDirty();
						repo.SaveNote(realnote);

						_log.Warn("Sync", "Resolve conflict: UseServerVersion");
					});
					break;
				case ConflictResolutionStrategy.UseClientCreateConflictFile:
					dispatcher.Invoke(() =>
					{
						if (realnote.IsLocalSaved)
						{
							realnote.OnAfterUpload(clonenote);
							realnote.ResetRemoteDirty();
							repo.SaveNote(realnote);
						}
						else
						{
							_log.Warn("Sync", "Resolve conflict: UseClientCreateConflictFile (do not [OnAfterUpload] cause note changed locally)");
						}

						var conflict = repo.CreateNewNote(conflictnote.Path);
						conflict.Title = string.Format("{0}_conflict-{1:yyyy-MM-dd_HH:mm:ss}", conflictnote.Title, DateTime.Now);
						conflict.Text = conflictnote.Text;
						conflict.Tags.Synchronize(conflictnote.Tags);
						conflict.IsConflictNote = true;
						repo.SaveNote(conflict);

						_log.Warn("Sync", "Resolve conflict: UseClientCreateConflictFile (conflictnote: " + conflict.UniqueName+ ")");
					});
					break;
				case ConflictResolutionStrategy.UseServerCreateConflictFile:
					dispatcher.Invoke(() =>
					{
						realnote.ApplyUpdatedData(clonenote);
						realnote.TriggerOnChanged(true);
						realnote.SetLocalDirty();
						realnote.ResetRemoteDirty();
						repo.SaveNote(realnote);

						var conflict = repo.CreateNewNote(conflictnote.Path);
						conflict.Title = string.Format("{0}_conflict-{1:yyyy-MM-dd_HH:mm:ss}", conflictnote.Title, DateTime.Now);
						conflict.Text = conflictnote.Text;
						conflict.Tags.Synchronize(conflictnote.Tags);
						conflict.IsConflictNote = true;
						repo.SaveNote(conflict);

						_log.Warn("Sync", "Resolve conflict: UseServerCreateConflictFile (conflictnote: " + conflict.UniqueName+ ")");
					});
					break;
				case ConflictResolutionStrategy.ManualMerge:
					dispatcher.Invoke(() =>
					{
						if (realnote.IsLocalSaved)
						{
							realnote.OnAfterUpload(clonenote);
							realnote.ResetRemoteDirty();
							repo.SaveNote(realnote);
						}
						else
						{
							_log.Warn("Sync", "Resolve conflict: ManualMerge (do not [OnAfterUpload] cause note changed locally)");
						}
						
						var txt0 = clonenote.Text;
						var txt1 = conflictnote.Text;

						var ttl0 = clonenote.Title;
						var ttl1 = conflictnote.Title;

						var tgs0 = clonenote.Tags.OrderBy(p=>p).ToList();
						var tgs1 = conflictnote.Tags.OrderBy(p=>p).ToList();

						var ndp0 = clonenote.Path;
						var ndp1 = conflictnote.Path;

						if (txt0 != txt1 || ttl0 != ttl1 || !tgs0.CollectionEquals(tgs1) || ndp0 != ndp1)
						{
							_log.Info(
								"Sync", 
								"Resolve conflict: ManualMerge :: Show dialog", 
								$"======== Title 1 ========\n{ttl0}\n\n======== Title 2 ========\n{ttl1}\n\n"+
								$"======== Text 1 ========\n{txt0}\n\n======== Text 2 ========\n{txt1}\n\n"+
								$"======== Tags 1 ========\n{string.Join(" | ", tgs0)}\n\n======== Tags 2 ========\n{string.Join(" | ", tgs1)}\n\n");

							dispatcher.BeginInvoke(() => 
							{
								repo.ShowConflictResolutionDialog(clonenote.UniqueName, txt0, ttl0, tgs0, ndp0, txt1, ttl1, tgs1, ndp1);
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

		private void DownloadNotes(List<Tuple<INote, INote>> notesToDownload, ref List<Tuple<string, Exception>> errors)
		{
			foreach (var noteuple in notesToDownload)
			{
				var realnote = noteuple.Item1;
				var clonenote = noteuple.Item2;

				_log.Info("Sync", string.Format("Download note {0}", clonenote.UniqueName));

				try
				{
					if (!clonenote.IsLocalSaved)
					{
						_log.Warn("Sync", "Downloading note skipped (unsaved changes)");
						continue;
					}

					var result = repo.Connection.UpdateNoteFromRemote(clonenote);

					switch (result)
					{
						case RemoteDownloadResult.UpToDate:
							_log.Info("Sync", "Downloading note -> UpToDate");
							if (realnote.ModificationDate != clonenote.ModificationDate)
							{
								_log.Info("Sync", "Downloading note -> UpToDate (but update local mdate)");
								dispatcher.Invoke(() =>
								{
									if (realnote.IsLocalSaved)
									{
										// Even when up to date - perhaps local mod date is wrong ...
										realnote.ModificationDate = clonenote.ModificationDate;
										realnote.ResetRemoteDirty();
									}
								});
							}
							break;

						case RemoteDownloadResult.Updated:
							_log.Info("Sync", "Downloading note -> Updated");
							dispatcher.Invoke(() =>
							{
								if (realnote.IsLocalSaved)
								{
									realnote.ApplyUpdatedData(clonenote);
									realnote.TriggerOnChanged(true);
									realnote.SetLocalDirty();
									realnote.ResetRemoteDirty();
									repo.SaveNote(realnote);
								}
								else
								{
									_log.Info("Sync", "Downloading (->Updated) skipped (unsaved local changes)");
								}
							});
							break;

						case RemoteDownloadResult.DeletedOnRemote:
							_log.Info("Sync", "Downloading note -> DeletedOnRemote");
							dispatcher.Invoke(() =>
							{
								if (realnote.IsLocalSaved)
								{
									repo.DeleteNote(realnote, false);
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
				}
				catch (Exception e)
				{
					var message = string.Format("Could not synchronize note '{2}' ({0}) cause of {1}", clonenote.UniqueName, e.Message, clonenote.Title);
					_log.Error("Sync", message, e);
					errors.Add(Tuple.Create(message, e));
				}
			}
		}

		private void DeleteNotes(List<INote> notesToDelete, ref List<Tuple<string, Exception>> errors)
		{
			foreach (var xnote in notesToDelete)
			{
				var note = xnote;

				_log.Info("Sync", string.Format("Delete note {0}", note.UniqueName));

				try
				{
					repo.Connection.DeleteNote(note);
					dispatcher.Invoke(() => repo.LocalDeletedNotes.Remove(note));
				}
				catch (Exception e)
				{
					var message = string.Format("Could not delete note {2} ({0}) on remote cause of {1}", note.UniqueName, e.Message, note.Title);
					_log.Error("Sync", message, e);
					errors.Add(Tuple.Create(message, e));
				}
			}
		}

		private void DownloadNewNotes(List<Tuple<INote, INote>> allNotes, ref List<Tuple<string, Exception>> errors)
		{
			var missing = repo.Connection.ListMissingNotes(allNotes.Select(p => p.Item2).ToList());

			foreach (var xnoteid in missing)
			{
				var noteid = xnoteid;

				_log.Info("Sync", string.Format("Download new note {{id:'{0}'}}", noteid));

				try
				{
					bool isnewnote;
					var note = repo.Connection.DownloadNote(noteid, out isnewnote);
					if (isnewnote)
					{
						note.SetLocalDirty();
						note.ResetRemoteDirty();
						dispatcher.Invoke(() => repo.AddNote(note, false));
					}
					else
					{
						_log.Warn("Sync", string.Format("Download new note {{id:'{0}'}} returned false", noteid));
					}
				}
				catch (Exception e)
				{
					var message = string.Format("Could not download new note '{0}' on remote cause of {1}", noteid, e.Message);
					_log.Error("Sync", message, e);
					errors.Add(Tuple.Create(message, e));
				}
			}
		}
		
		public void SyncNowAsync()
		{
			_log.Info("Sync", "Requesting priorty sync");
			RequestPrioritySync();
		}

		public void StopAsync()
		{
			using (dispatcher.EnableCustomDispatcher())
			{
				_log.Info("Sync", "Requesting stop");

				if (isSyncing)
				{
					_log.Info("Sync", "Requesting stop (early exit due to isSyncing)");

					RequestCancel();
					WaitForSyncStop(60);

					return;
				}

				prioritysync = false;
				RequestCancel();
				WaitForStopped(45);
			}
		}

		public void SyncNowAndStopAsync()
		{
			using (dispatcher.EnableCustomDispatcher())
			{
				_log.Info("Sync", "Requesting sync&stop");

				if (isSyncing)
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
			var _startWait1 = Environment.TickCount;
			for (;;)
			{
				if (!running)
				{
					_log.Info("Sync", "Waiting for SyncThread stopping :: finished (running=false)");
					return;
				}

				if (isSyncing)
				{
					_log.Info("Sync", "Waiting for SyncThread stopping :: currently syncing (isSyncing=true)");

					RequestCancel();
					WaitForSyncStop(30);

					_log.Info("Sync", "Waiting for SyncThread stopping :: syncing finished (isSyncing=true)");

					RequestCancel();
				}

				SleepDoEvents(10);

				if (Environment.TickCount - _startWait1 > seconds * 1000)
				{
					_log.Error("Sync", "Requesting sync&stop failed after timeout (waiting on prioritysync)");
					throw new Exception("Background thread timeout after "+ seconds + "sec");
				}
			}
		}

		private void WaitForSyncStop(int seconds)
		{
			var _startWait2 = Environment.TickCount;
			for (;;)
			{
				if (!isSyncing)
				{
					_log.Info("Sync", "Requesting sync&stop finished (isSyncing=false)");
					return;
				}
				if (!running)
				{
					_log.Info("Sync", "Requesting sync&stop finished (running=false)");
					return;
				}
				SleepDoEvents(10);

				if (Environment.TickCount - _startWait2 > seconds * 1000)
				{
					_log.Error("Sync", "Requesting sync&stop failed after timeout (waiting on isSyncing)");
					throw new Exception("Background thread timeout after "+ seconds + "sec");
				}
			}
		}

		private void RequestCancel()
		{
			cancel = true;
			_comChannel.Set();
		}

		private void RequestPrioritySync()
		{
			prioritysync = true;
			_comChannel.Set();
		}

		public void Kill()
		{
			//if (thread != null) thread.Abort(); //TODO
		}

		private void SleepDoEvents(int sleep)
		{
			Thread.Sleep(sleep);
			dispatcher.Work();
		}
	}
}
