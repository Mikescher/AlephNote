using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Documents;

namespace AlephNote.Repository
{
	class SynchronizationThread
	{
		private readonly NoteRepository repo;
		private readonly ISynchronizationFeedback listener;
		private int delay;

		private Thread thread;

		private bool prioritysync = false;
		private bool cancel = false;
		private bool running = false;
		private bool isSyncing = false;
		
		public SynchronizationThread(NoteRepository repository, ISynchronizationFeedback synclistener)
		{
			repo = repository;
			listener = synclistener;
		}

		public void Start(int syncdelay)
		{
			if (running) throw new Exception("SynchronizationThread already running");

			delay = syncdelay;
			cancel = false;
			thread = new Thread(ThreadRun);
			thread.IsBackground = true;
			thread.Start();
		}

		private void ThreadRun()
		{
			running = true;

			for (; ; )
			{
				isSyncing = true;
				DoSync();
				isSyncing = false;

				var tick = Environment.TickCount;
				do
				{
					if (cancel) { running = false; return; }
					if (prioritysync) { prioritysync = false; break; }
					
					Thread.Sleep(333);

				} while (Environment.TickCount - tick < delay);
			}
		}

		private void DoSync()
		{
			List<Tuple<string, Exception>> errors = new List<Tuple<string, Exception>>();

			BeginInvoke(() => listener.StartSync());

			try
			{

				repo.Connection.StartSync();
				{
					List<Tuple<INote, INote>> allNotes = new List<Tuple<INote, INote>>();
					List<Tuple<INote, INote>> notesToUpload = new List<Tuple<INote, INote>>();
					List<Tuple<INote, INote>> notesToDownload = new List<Tuple<INote, INote>>();
					List<INote> notesToDelete = new List<INote>();
					Invoke(() =>
					{
						allNotes = repo.Notes.Select(p => Tuple.Create(p, p.Clone())).ToList();
						notesToUpload = allNotes.Where(p => repo.Connection.NeedsUpload(p.Item2)).ToList();
						notesToDownload = allNotes.Where(p => repo.Connection.NeedsDownload(p.Item2)).ToList();
						notesToDelete = repo.LocalDeletedNotes.ToList();
					});

					UploadNotes(notesToUpload, ref errors);

					DownloadNotes(notesToDownload, ref errors);

					DeleteNotes(notesToDelete, ref errors);

					DownloadNewNotes(allNotes, ref errors);
				}
				repo.Connection.FinishSync();
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
		}

		private void UploadNotes(List<Tuple<INote, INote>> notesToUpload, ref List<Tuple<string, Exception>> errors)
		{
			foreach (var noteuple in notesToUpload)
			{
				var realnote = noteuple.Item1;
				var clonenote = noteuple.Item2;

				try
				{
					if (!clonenote.IsLocalSaved)
					{
						Invoke(() =>
						{
							if (!realnote.IsLocalSaved) repo.SaveNote(realnote);
						});
					}

					clonenote = repo.Connection.UploadNote(clonenote);

					Invoke(() =>
					{
						if (realnote.IsLocalSaved)
						{
							realnote.OnAfterUpload(clonenote);
							repo.SaveNote(realnote);
							realnote.IsRemoteSaved = true;
						}
					});
				}
				catch (Exception e)
				{
					errors.Add(Tuple.Create(string.Format("Could not upload note '{2}' ({0}) cause of {1}", clonenote.GetUniqueName(), e.Message, clonenote.Title), e));
				}
			}
		}

		private void DownloadNotes(List<Tuple<INote, INote>> notesToDownload, ref List<Tuple<string, Exception>> errors)
		{
			foreach (var noteuple in notesToDownload)
			{
				var realnote = noteuple.Item1;
				var clonenote = noteuple.Item2;

				try
				{
					if (!clonenote.IsLocalSaved) continue;

					var result = repo.Connection.UpdateNote(clonenote);

					switch (result)
					{
						case RemoteResult.UpToDate:
							// OK
							break;

						case RemoteResult.Updated:
							Invoke(() =>
							{
								if (realnote.IsLocalSaved)
								{
									realnote.ApplyUpdatedData(clonenote);
									realnote.TriggerOnChanged();
									realnote.SetLocalDirty();
									realnote.ResetRemoteDirty();
								}
							});
							break;

						case RemoteResult.DeletedOnRemote:
							Invoke(() =>
							{
								if (realnote.IsLocalSaved)
								{
									repo.DeleteNote(realnote, false);
								}
							});
							break;

						default:
							throw new ArgumentOutOfRangeException();
					}
				}
				catch (Exception e)
				{
					errors.Add(Tuple.Create(string.Format("Could not synchronize note '{2}' ({0}) cause of {1}", clonenote.GetUniqueName(), e.Message, clonenote.Title), e));
				}
			}
		}

		private void DeleteNotes(List<INote> notesToDelete, ref List<Tuple<string, Exception>> errors)
		{
			foreach (var xnote in notesToDelete)
			{
				var note = xnote;

				try
				{
					repo.Connection.DeleteNote(note);
					Invoke(() => repo.LocalDeletedNotes.Remove(note));
				}
				catch (Exception e)
				{
					errors.Add(Tuple.Create(string.Format("Could not delete note {2} ({0}) on remote cause of {1}", note.GetUniqueName(), e.Message, note.Title), e));
				}
			}
		}

		private void DownloadNewNotes(List<Tuple<INote, INote>> allNotes, ref List<Tuple<string, Exception>> errors)
		{
			var missing = repo.Connection.ListMissingNotes(allNotes.Select(p => p.Item2).ToList());

			foreach (var xnoteid in missing)
			{
				var noteid = xnoteid;

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
				}
				catch (Exception e)
				{
					errors.Add(Tuple.Create(string.Format("Could not download new note '{0}' on remote cause of {1}", noteid, e.Message), e));
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
			prioritysync = true;
		}

		public void SyncNowAndStopAsync()
		{
			if (isSyncing)
			{
				cancel = true;
				return;
			}

			prioritysync = true;
			for (int i = 0; i < 100; i++)
			{
				if (!running) return;
				if (!prioritysync)
				{
					cancel = true;
					return;
				}

				Thread.Sleep(100);
			}
			throw new Exception("Background thread timeout after 10sec");
		}
	}
}
