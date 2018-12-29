using AlephNote.Common.Plugins;
using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using AlephNote.Impl;
using AlephNote.PluginInterface.Util;
using AlephNote.Common.Util;
using MSHC.WPF.MVVM;

namespace AlephNote.WPF.Windows
{
	class FirstStartupViewmodel : ObservableObject
	{
		public string Appversion { get { return App.AppVersionProperty; } }

		public IEnumerable<IRemotePlugin> AvailableProvider => PluginManager.Inst.LoadedPlugins;
		public object Null => null;

		private IRemotePlugin _selectedProvider;
		public IRemotePlugin SelectedProvider
		{
			get { return _selectedProvider; }
			set { _selectedProvider = value; OnPropertyChanged(); OnPluginChanged(); }
		}

		private RemoteStorageAccount _account = null;
		public RemoteStorageAccount Account
		{
			get { return _account; }
			set { _account = value; OnPropertyChanged(); OnAccountChanged(); }
		}

		private bool _configurationValidated = false;
		public bool ConfigurationValidated
		{
			get { return _configurationValidated; }
			set { _configurationValidated = value; OnPropertyChanged(); }
		}

		private int _syncProgress = 0;
		public int SyncProgress
		{
			get { return _syncProgress; }
			set { _syncProgress = value; OnPropertyChanged(); }
		}

		private bool _isValidating = false;
		public bool IsValidating
		{
			get { return _isValidating; }
			set { _isValidating = value; OnPropertyChanged(); }
		}

		private string _syncInfoText = "";
		public string SyncInfoText
		{
			get { return _syncInfoText; }
			set { _syncInfoText = value; OnPropertyChanged(); }
		}
		
		private Thread _syncThread     = null;
		private Thread _progressThread = null;

		public List<INote> ValidationResultNotes = null;
		public IRemoteStorageSyncPersistance ValidationResultData = null;

		private readonly FirstStartupWindow _owner;

		public FirstStartupViewmodel(FirstStartupWindow o)
		{
			var guidLocal      = Guid.Parse("37de6de1-26b0-41f5-b252-5e625d9ecfa3");
			var guidSimpleNote = Guid.Parse("4c73e687-3803-4078-9bf0-554aaafc0873");

			SelectedProvider = AvailableProvider.FirstOrDefault(p => p.GetUniqueID() == guidSimpleNote) ??
							   AvailableProvider.FirstOrDefault(p => p.GetUniqueID() == guidLocal) ??
							   AvailableProvider.FirstOrDefault();

			_owner = o;
		}

		private void OnPluginChanged()
		{
			ConfigurationValidated = false;
			ValidationResultData = null;
			ValidationResultNotes = null;

			if (SelectedProvider != null)
				Account = new RemoteStorageAccount(Guid.NewGuid(), SelectedProvider, SelectedProvider.CreateEmptyRemoteStorageConfiguration());
			else
				Account = null;

			if (_syncThread != null && _syncThread.IsAlive) _syncThread.Abort();
			if (_progressThread != null && _progressThread.IsAlive) _progressThread.Abort();

			SyncProgress = 0;
			SyncInfoText = string.Empty;
		}

		public void OnAccountChanged()
		{
			ConfigurationValidated = false;
			ValidationResultData = null;
			ValidationResultNotes = null;

			if (_syncThread != null && _syncThread.IsAlive) _syncThread.Abort();
			if (_progressThread != null && _progressThread.IsAlive) _progressThread.Abort();

			SyncProgress = 0;
			SyncInfoText = string.Empty;
		}

		public void StartSync()
		{
			if (SelectedProvider == null) return;
			if (Account == null) return;

			if (_syncThread != null && _syncThread.IsAlive) _syncThread.Abort();
			if (_progressThread != null && _progressThread.IsAlive) _progressThread.Abort();

			SyncInfoText = "Starting Synchronization";
			var acc = new RemoteStorageAccount(Account.ID, Account.Plugin, Account.Config);
			_syncThread = new Thread(() =>
			{
				try
				{
					Application.Current.Dispatcher.Invoke(() => { IsValidating = true; });
					
					var r = DoSync(acc, App.Logger);
					Application.Current.Dispatcher.Invoke(() =>
					{
						ConfigurationValidated = true;
						ValidationResultData = r.Item1;
						ValidationResultNotes = r.Item2;
						SyncProgress = -100;
						SyncInfoText = string.Empty;
					});
				}
				catch (ThreadAbortException)
				{
					Application.Current.Dispatcher.Invoke(() =>
					{
						ConfigurationValidated = false;
						ValidationResultData = null;
						ValidationResultNotes = null;
						SyncProgress = 0;
						SyncInfoText = string.Empty;
					});
					return;
				}
				catch (Exception e)
				{
					Application.Current.Dispatcher.Invoke(() =>
					{
						ConfigurationValidated = false;
						ValidationResultData = null;
						ValidationResultNotes = null;
						SyncProgress = -66;
						SyncInfoText = string.Empty;
					});
					Application.Current.Dispatcher.BeginInvoke(new Action(() =>
					{
						SyncProgress = -66;
						SyncErrorDialog.Show(_owner, e);
					}));
				}
				finally
				{
					Application.Current.Dispatcher.BeginInvoke(new Action(() => 
					{
						IsValidating = false;
						SyncInfoText = string.Empty;
					}));
				}
			});
			_progressThread = new Thread(() =>
			{
				Thread.Sleep(350);
				for (;;)
				{
					if (!_syncThread.IsAlive) break;
					Thread.Sleep(150);
					Application.Current.Dispatcher.Invoke(() => { SyncProgress++; });
				}
				Application.Current.Dispatcher.Invoke(() =>
				{
					if (ConfigurationValidated) SyncProgress = -100; else SyncProgress = -66;
					SyncInfoText = string.Empty;
				});
			});

			_syncThread.Start();
			_progressThread.Start();
		}

		private Tuple<IRemoteStorageSyncPersistance, List<INote>> DoSync(RemoteStorageAccount acc, AlephLogger log)
		{
			var data = SelectedProvider.CreateEmptyRemoteSyncData();

			var conn = acc.Plugin.CreateRemoteStorageConnection(PluginManagerSingleton.Inst.GetProxyFactory().Build(), acc.Config, new HierachyEmulationConfig(false, "\\", '\\'));

			var resultNotes = new List<INote>();

			Application.Current.Dispatcher.Invoke(() => { SyncInfoText = "Connect to remote"; });
			conn.StartSync(data, new List<INote>(), new List<INote>());
			{
				Application.Current.Dispatcher.Invoke(() => { SyncInfoText = "List notes from remote"; });
				var missing = conn.ListMissingNotes(new List<INote>());

				int idx = 0;
				foreach (var xnoteid in missing)
				{
					var noteid = xnoteid;
					idx++;

					try
					{
						string msg = $"Download Note {idx}/{missing.Count}";
						Application.Current.Dispatcher.Invoke(() => { SyncInfoText = msg; });

						var note = conn.DownloadNote(noteid, out var isnewnote);
						if (isnewnote)
						{
							note.SetLocalDirty("Set Note LocalDirty=true after download in Startupmode");
							note.ResetRemoteDirty("Set Note RemoteDirty=false after download in Startupmode");
							resultNotes.Add(note);
						}
						else
						{
							log.Warn("Sync_FirstStart", string.Format("Download new note {{id:'{0}'}} returned false", noteid));
						}
					}
					catch (ThreadAbortException)
					{
						throw;
					}
					catch (Exception e)
					{
						throw new Exception(string.Format("Could not download new note '{0}' on remote cause of {1}", noteid, e.Message));
					}
				}
			}
			Application.Current.Dispatcher.Invoke(() => { SyncInfoText = "Finish synchronization"; });
			conn.FinishSync();

			return Tuple.Create(data, resultNotes);
		}
	}
}
