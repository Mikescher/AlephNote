using AlephNote.Repository;
using System;
using System.Collections.Generic;

namespace AlephNote.Plugins
{
	static class GithubConnection
	{
#pragma warning disable 0649
// ReSharper disable All
		private class JsonResponseAsset { public string browser_download_url; }
		private class JsonResponse { public string tag_name; public DateTime published_at; public List<JsonResponseAsset> assets; }
// ReSharper restore All
#pragma warning restore 0649

		public static Tuple<Version, DateTime, string> GetLatestRelease()
		{
			var rest = new SimpleJsonRest(null, @"https://api.github.com", App.Logger);

			var response = rest.Get<JsonResponse>("repos/Mikescher/AlephNote/releases/latest");

			var url = response.assets[0].browser_download_url;
			var date = response.published_at;
			var version = ParseVersion(response.tag_name);

			return Tuple.Create(version, date, url);
		}

		private static Version ParseVersion(string dat)
		{
			dat = dat.Trim().ToLower();
			if (dat.StartsWith("v")) dat = dat.Substring(1);
			return Version.Parse(dat);
		}
	}
}
