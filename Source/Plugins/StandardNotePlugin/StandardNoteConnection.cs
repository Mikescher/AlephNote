using AlephNote.PluginInterface;
using System;
using System.Net;

namespace AlephNote.Plugins.StandardNote
{
	/// <summary>
	/// https://github.com/standardnotes/doc/blob/master/Client%20Development%20Guide.md
	/// http://standardfile.org/api-auth#api
	/// </summary>
	class StandardNoteConnection : IRemoteStorageConnection
	{
		private readonly StandardNoteConfig _config;
		private readonly IWebProxy _proxy;

		private StandardNoteAPI.APIResultAuthorize _token = null;

		public StandardNoteConnection(IWebProxy proxy, StandardNoteConfig config)
		{
			_config = config;
			_proxy = proxy;
		}

		private void RefreshToken()
		{
			try
			{
				if (_token == null)
					_token = StandardNoteAPI.Authenticate(_proxy, _config.Server, _config.Email, _config.Password);
			}
			catch (Exception e)
			{
				throw new Exception("Could not authenticate with SimpleNote server : " + e.Message, e);
			}
		}
		
	}
}
