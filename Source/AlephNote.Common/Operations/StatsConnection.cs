using System;
using AlephNote.Common.Network;
using AlephNote.Common.Repository;
using AlephNote.Common.Settings;
using AlephNote.Common.Util;
using AlephNote.PluginInterface;

namespace AlephNote.Common.Operations
{
	public class StatsConnection
	{
#pragma warning disable 0649
// ReSharper disable All
		private class JsonResponseAsset { public string browser_download_url; }
		private class JsonResponse { public bool success; }
// ReSharper restore All
#pragma warning restore 0649
			
		private readonly AppSettings _settings;
		private readonly NoteRepository _repository;

		public StatsConnection(AppSettings settings, NoteRepository repo)
		{
			_settings = settings;
			_repository = repo;
		}

		public bool UploadStatistics(Version version)
		{
			try
			{
				var rest = new SimpleJsonRest(_settings.CreateProxy(), @"https://mikescher.com");

				var response = rest.Get<JsonResponse>(
					"api/statsping", 
					$"Name={"AlephNote"}", 
					$"Version={version}", 
					$"ClientID={_settings.ClientID:D}", 
					$"ProviderStr={_settings.ActiveAccount.Plugin.DisplayTitleShort.Replace(' ', '_')}", 
					$"ProviderID={_settings.ActiveAccount.Plugin.GetUniqueID()}",
					$"NoteCount={_repository.Notes.Count}");

				return response.success;

			}
			catch (Exception e)
			{
				LoggerSingleton.Inst.Warn("StatsConnection", "Could not send anonymous usage statistics to server", e.ToString());
				return false;
			}
		}

		private static Version ParseVersion(string dat)
		{
			dat = dat.Trim().ToLower();
			if (dat.StartsWith("v")) dat = dat.Substring(1);
			return Version.Parse(dat);
		}
	}
}
