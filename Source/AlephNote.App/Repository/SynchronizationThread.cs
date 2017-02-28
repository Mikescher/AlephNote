using AlephNote.PluginInterface;
using MSHC.Lang.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;

namespace AlephNote.Repository
{
	class SynchronizationThread
	{
		private readonly NoteRepository repo;
		private readonly ISynchronizationFeedback listener;
		private readonly ConflictResolutionStrategy conflictStrategy;
		private int delay;

		private static readonly object _syncobj = new object();
		private Thread thread;

		private bool prioritysync = false;
		private bool cancel = false;
		private bool running = false;
		private bool isSyncing = false;
		
		public SynchronizationThread(NoteRepository repository, ISynchronizationFeedback synclistener, ConflictResolutionStrategy strat)
		{
			repo = repository;
			listener = synclistener;
			conflictStrategy = strat;
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
					DoSync();
					isSyncing = false;
				}

				var tick = Environment.TickCount;
				do
				{
					if (cancel) { running = false; App.Logger.Info("Sync", "Thread cancelled"); return; }
					if (prioritysync) { prioritysync = false; App.Logger.Info("Sync", "Thread ffwd (priority sync)"); break; }
					
					Thread.Sleep(333);

				} while (Environment.TickCount - tick < delay);
			}
		}

		private void DoSync()
		{
			App.Logger.Info("Sync", "Starting remote synchronization");

			List<Tuple<string, Exception>> errors = new List<Tuple<string, Exception>>();

			BeginInvoke(() => listener.StartSync());

			try
			{
				var data = repo.GetSyncData();

				List<Tuple<INote, INote>> allNotes = new List<Tuple<INote, INote>>();
				List<INote> notesToDelete = new List<INote>();
				Invoke(() =>
				{
					allNotes = repo.Notes.Select(p => Tuple.Create(p, p.Clone())).ToList();
					notesToDelete = repo.LocalDeletedNotes.ToList();
				});

				App.Logger.Info("Sync", string.Format("Found {0} alive notes and {1} deleted notes", allNotes.Count, notesToDelete.Count));

				repo.Connection.StartSync(data, allNotes.Select(p => p.Item2).ToList(), notesToDelete);
				{
					var notesToUpload = allNotes.Where(p => repo.Connection.NeedsUpload(p.Item2)).ToList();
					var notesToDownload = allNotes.Where(p => repo.Connection.NeedsDownload(p.Item2)).ToList();

					App.Logger.Info("Sync", string.Format("Found {0} notes for upload and {1} notes for download", notesToUpload.Count, notesToDownload.Count));

					UploadNotes(notesToUpload, ref errors);

					DownloadNotes(notesToDownload, ref errors);

					DeleteNotes(notesToDelete, ref errors);

					DownloadNewNotes(allNotes, ref errors);
				}
				repo.Connection.FinishSync();

				repo.WriteSyncData(data);
			}
			catch (Exception e)
			{
				errors.Add(Tuple.Create("Execption while syncing notes: " + e.Message, e));
			}

			if (errors.Any())
			{
				BeginInvoke(() => listener.SyncError(errors));
			}
			else
			{
				BeginInvoke(() => listener.SyncSuccess(DateTimeOffset.Now));
			}

			App.Logger.Info("Sync", "Finished remote synchronization");
		}

		private void UploadNotes(List<Tuple<INote, INote>> notesToUpload, ref List<Tuple<string, Exception>> errors)
		{
			foreach (var notetuple in notesToUpload)
			{
				var realnote = notetuple.Item1;
				var clonenote = notetuple.Item2;

				App.Logger.Info("Sync", string.Format("Upload note {0}", clonenote.GetUniqueName()));

				try
				{
					if (!clonenote.IsLocalSaved)
					{
						Invoke(() =>
						{
							if (!realnote.IsLocalSaved) repo.SaveNote(realnote);
						});
					}

					INote conflictnote;
					var result = repo.Connection.UploadNoteToRemote(ref clonenote, out conflictnote, conflictStrategy);

					switch (result)
					{
						case RemoteUploadResult.UpToDate:
						case RemoteUploadResult.Uploaded:
							Invoke(() =>
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
							Invoke(() =>
							{
								realnote.ApplyUpdatedData(clonenote);
								realnote.TriggerOnChanged(true);
								realnote.SetLocalDirty();
								realnote.ResetRemoteDirty();
							});
							break;

						case RemoteUploadResult.Conflict:
							App.Logger.Warn("Sync", "Uploading note " + clonenote.GetUniqueName() + " resulted in conflict");
							ResolveUploadConflict(realnote, clonenote, conflictnote);
							break;

						default:
							throw new ArgumentOutOfRangeException();
					}

				}
				catch (Exception e)
				{
					var message = string.Format("Could not upload note '{2}' ({0}) cause of {1}", clonenote.GetUniqueName(), e.Message, clonenote.Title);

					App.Logger.Error("Sync", message, e);
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
					Invoke(() =>
					{
						if (realnote.IsLocalSaved)
						{
							realnote.OnAfterUpload(clonenote);
							realnote.ResetRemoteDirty();
							repo.SaveNote(realnote);

							App.Logger.Warn("Sync", "Resolve conflict: UseClientVersion");
						}
						else
						{
							App.Logger.Warn("Sync", "Resolve conflict: UseClientVersion (do nothing cause note changed locally)");
						}
					});
					break;
				case ConflictResolutionStrategy.UseServerVersion:
					Invoke(() =>
					{
						realnote.ApplyUpdatedData(clonenote);
						realnote.TriggerOnChanged(true);
						realnote.SetLocalDirty();
						realnote.ResetRemoteDirty();
						repo.SaveNote(realnote);

						App.Logger.Warn("Sync", "Resolve conflict: UseServerVersion");
					});
					break;
				case ConflictResolutionStrategy.UseClientCreateConflictFile:
					Invoke(() =>
					{
						if (realnote.IsLocalSaved)
						{
							realnote.OnAfterUpload(clonenote);
							realnote.ResetRemoteDirty();
							repo.SaveNote(realnote);

							var conflict = repo.CreateNewNote();
							conflict.Title = string.Format("{0}_conflict-{1:yyyy-MM-dd_HH:mm:ss}", conflictnote.Title, DateTime.Now);
							conflict.Text = conflictnote.Text;
							conflict.Tags.Synchronize(conflictnote.Tags);
							conflict.IsConflictNote = true;
							repo.SaveNote(conflict);

							App.Logger.Warn("Sync", "Resolve conflict: UseClientCreateConflictFile (conflictnote: " + conflict.GetUniqueName() + ")");
						}
						else
						{
							App.Logger.Warn("Sync", "Resolve conflict: UseClientCreateConflictFile");
						}
					});
					break;
				case ConflictResolutionStrategy.UseServerCreateConflictFile:
					Invoke(() =>
					{
						realnote.ApplyUpdatedData(clonenote);
						realnote.TriggerOnChanged(true);
						realnote.SetLocalDirty();
						realnote.ResetRemoteDirty();
						repo.SaveNote(realnote);

						var conflict = repo.CreateNewNote();
						conflict.Title = string.Format("{0}_conflict-{1:yyyy-MM-dd_HH:mm:ss}", conflictnote.Title, DateTime.Now);
						conflict.Text = conflictnote.Text;
						conflict.Tags.Synchronize(conflictnote.Tags);
						conflict.IsConflictNote = true;
						repo.SaveNote(conflict);

						App.Logger.Warn("Sync", "Resolve conflict: UseServerCreateConflictFile (conflictnote: " + conflict.GetUniqueName() + ")");
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

				App.Logger.Info("Sync", string.Format("Download note {0}", clonenote.GetUniqueName()));

				try
				{
					if (!clonenote.IsLocalSaved)
					{
						App.Logger.Warn("Sync", "Downloading note skipped (unsaved changes)");
						continue;
					}

					var result = repo.Connection.UpdateNoteFromRemote(clonenote);

					switch (result)
					{
						case RemoteDownloadResult.UpToDate:
							App.Logger.Info("Sync", "Downloading note -> UpToDate");
							if (realnote.ModificationDate != clonenote.ModificationDate)
							{
								App.Logger.Info("Sync", "Downloading note -> UpToDate (but update local mdate)");
								Invoke(() =>
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
							App.Logger.Info("Sync", "Downloading note -> Updated");
							Invoke(() =>
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
									App.Logger.Info("Sync", "Downloading (->Updated) skipped (unsaved local changes)");
								}
							});
							break;

						case RemoteDownloadResult.DeletedOnRemote:
							App.Logger.Info("Sync", "Downloading note -> DeletedOnRemote");
							Invoke(() =>
							{
								if (realnote.IsLocalSaved)
								{
									repo.DeleteNote(realnote, false);
								}
								else
								{
									App.Logger.Info("Sync", "Downloading (->DeletedOnRemote) skipped (unsaved local changes)");
								}
							});
							break;

						default:
							throw new ArgumentOutOfRangeException();
					}
				}
				catch (Exception e)
				{
					var message = string.Format("Could not synchronize note '{2}' ({0}) cause of {1}", clonenote.GetUniqueName(), e.Message, clonenote.Title);
					App.Logger.Error("Sync", message, e);
					errors.Add(Tuple.Create(message, e));
				}
			}
		}

		private void DeleteNotes(List<INote> notesToDelete, ref List<Tuple<string, Exception>> errors)
		{
			foreach (var xnote in notesToDelete)
			{
				var note = xnote;

				App.Logger.Info("Sync", string.Format("Delete note {0}", note));

				try
				{
					repo.Connection.DeleteNote(note);
					Invoke(() => repo.LocalDeletedNotes.Remove(note));
				}
				catch (Exception e)
				{
					var message = string.Format("Could not delete note {2} ({0}) on remote cause of {1}", note.GetUniqueName(), e.Message, note.Title);
					App.Logger.Error("Sync", message, e);
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

				App.Logger.Info("Sync", string.Format("Download new note {{id:'{0}'}}", noteid));

				try
				{
					bool isnewnote;
					var note = repo.Connection.DownloadNote(noteid, out isnewnote);
					if (isnewnote)
					{
						note.SetLocalDirty();
						note.ResetRemoteDirty();
						Invoke(() => repo.AddNote(note, false));
					}
					else
					{
						App.Logger.Warn("Sync", string.Format("Download new note {{id:'{0}'}} returned false", noteid));
					}
				}
				catch (Exception e)
				{
					var message = string.Format("Could not download new note '{0}' on remote cause of {1}", noteid, e.Message);
					App.Logger.Error("Sync", message, e);
					errors.Add(Tuple.Create(message, e));
				}
			}
		}

		private void Invoke(Action a)
		{
			var app = Application.Current;
			app.Dispatcher.Invoke(a);
		}

		private void BeginInvoke(Action a)
		{
			var app = Application.Current;
			app.Dispatcher.BeginInvoke(a);
		}
		
		public void SyncNowAsync()
		{
			App.Logger.Info("Sync", "Requesting priorty sync");
			prioritysync = true;
		}

		public void StopAsync()
		{
			App.Logger.Info("Sync", "Requesting stop");

			if (isSyncing)
			{
				App.Logger.Info("Sync", "Requesting stop (early exit due to isSyncing)");

				cancel = true;
				return;
			}

			prioritysync = false;
			cancel = true;
			for (int i = 0; i < 100; i++)
			{
				if (!running)
				{
					App.Logger.Info("Sync", "Requesting stop - finished waiting (running=false)");
					return;
				}

				if (isSyncing)
				{
					App.Logger.Info("Sync", "Requesting stop - abort waiting (isSyncing=true)");
					return;
				}

				Thread.Sleep(100);
			}

			App.Logger.Error("Sync", "Requesting stop failed after timeout");
			throw new Exception("Background thread timeout after 10sec");
		}

		public void SyncNowAndStopAsync()
		{
			App.Logger.Info("Sync", "Requesting sync&stop");

			if (isSyncing)
			{
				App.Logger.Info("Sync", "Requesting sync&stop (early exit due to isSyncing)");

				cancel = true;
				return;
			}

			prioritysync = true;
			for (int i = 0; i < 100; i++)
			{
				if (!running)
				{
					App.Logger.Info("Sync", "Requesting sync&stop stop waiting (running=false)");
					return;
				}

				if (!prioritysync)
				{
					App.Logger.Info("Sync", "Requesting sync&stop stop waiting (prioritysync=false)");
					cancel = true;
					return;
				}

				Thread.Sleep(100);
			}

			App.Logger.Error("Sync", "Requesting sync&stop failed after timeout");
			throw new Exception("Background thread timeout after 10sec");
		}
	}
}
