using AlephNote.PluginInterface;

namespace AlephNote.Common.Repository
{
	public interface IRepository
	{
		bool SupportsPinning { get; }
		bool SupportsLocking { get; }
		bool SupportsTags { get; }
		bool SupportsDownloadMultithreading { get; }
		bool SupportsNewDownloadMultithreading { get; }
		bool SupportsUploadMultithreading { get; }

		IRemoteStorageConnection Connection { get; }

		void Shutdown(bool lastSync = true);
		void SyncNow();
		void SaveAll();
		void DeleteLocalData();
		void StartSync();
	}
}
