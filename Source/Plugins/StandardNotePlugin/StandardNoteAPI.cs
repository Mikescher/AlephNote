using AlephNote.PluginInterface;
using MSHC.Math.Encryption;
using MSHC.Network;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Text;

namespace AlephNote.Plugins.StandardNote
{
	/// <summary>
	/// https://github.com/standardnotes/doc/blob/master/Client%20Development%20Guide.md
	/// http://standardfile.org/api#api
	/// </summary>
	static class StandardNoteAPI
	{
#pragma warning disable 0649
		// ReSharper disable All
		public enum PasswordAlg { sha512, sha256 }
		public enum PasswordFunc { pbkdf2 }

		public class APIAuthParams { public string pw_salt; public PasswordAlg pw_alg; public PasswordFunc pw_func; public int pw_cost, pw_key_size; }
		public class APIResultUser { public Guid uuid; public string email; }
		public class APIResultAuthorize { public APIResultUser user; public string token; }
		// ReSharper restore All
#pragma warning restore 0649

		private static WebClient CreateClient(IWebProxy proxy, string authToken)
		{
			var web = new GZWebClient();
			if (proxy != null) web.Proxy = proxy;
			web.Headers["User-Agent"] = "AlephNote/1.0.0.0";
			if (authToken != null) web.Headers["X-Simperium-Token"] = authToken;
			return web;
		}

		private static string CreateUri(string host, string path, string parameter = "")
		{
			if (!host.ToLower().StartsWith("http")) host = "http://" + host;

			if (!host.EndsWith("/")) host = host + "/";
			if (path.StartsWith("/")) path = path.Substring(1);

			if (path.EndsWith("/")) path = path.Substring(path.Length);

			if (parameter != "")
				return host + path + "?" + parameter;
			else
				return host + path;
		}

		public static APIResultAuthorize Authenticate(IWebProxy proxy, string host, string mail, string password, IAlephLogger logger)
		{
			string result;

			try
			{
				var uri = CreateUri(host, "auth/params", "email=" + mail);

				logger.Debug(StandardNotePlugin.Name, "Request '" + uri + "'");

				result = CreateClient(proxy, null).DownloadString(uri);
			}
			catch (Exception e)
			{
				throw new StandardNoteAPIException("Communication with StandardNoteAPI failed", e);
			}

			try
			{
				var apiparams = JsonConvert.DeserializeObject<APIAuthParams>(result);
				
				logger.Debug(StandardNotePlugin.Name, "AutParams.pw_func: " + apiparams.pw_func);
				logger.Debug(StandardNotePlugin.Name, "AutParams.pw_alg: " + apiparams.pw_alg);
				logger.Debug(StandardNotePlugin.Name, "AutParams.pw_cost: " + apiparams.pw_cost);
				logger.Debug(StandardNotePlugin.Name, "AutParams.pw_key_size: " + apiparams.pw_key_size);

				if (apiparams.pw_func != PasswordFunc.pbkdf2) throw new Exception("Unknown pw_func: " + apiparams.pw_func);

				byte[] bytes; 

				if (apiparams.pw_alg == PasswordAlg.sha512)
				{
					bytes = PBKDF2.GenerateDerivedKey(apiparams.pw_key_size / 8, Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(apiparams.pw_salt), apiparams.pw_cost, PBKDF2.HMACType.SHA512);
				}
				else if (apiparams.pw_alg == PasswordAlg.sha512)
				{
					bytes = PBKDF2.GenerateDerivedKey(apiparams.pw_key_size / 8, Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(apiparams.pw_salt), apiparams.pw_cost, PBKDF2.HMACType.SHA512);
				}
				else
				{
					throw new Exception("Unknown pw_alg: " + apiparams.pw_alg);
				}

				var pw = bytes.Take(bytes.Length / 2).ToArray();
				var mk = bytes.Skip(bytes.Length / 2).ToArray();

				var uri = CreateUri(host, "auth/sign_in", "email=" + mail + "&password=" + EncodingConverter.ByteToHexBitFiddle(pw).ToLower());

				result = CreateClient(proxy, null).UploadString(uri, string.Empty);

				return JsonConvert.DeserializeObject<APIResultAuthorize>(result);

			}
			catch (Exception e)
			{
				throw new StandardNoteAPIException("Authentification with StandardNoteAPI failed.\r\nHTTP-Response:\r\n" + result, e);
			}
		}
		
	}
}
