using MSHC.Math.Encryption;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace AlephNote.Plugins.StandardNote
{
	/// <summary>
	/// https://github.com/standardnotes/doc/blob/master/Client%20Development%20Guide.md
	/// </summary>
	static class SimpleNoteAPI
	{
#pragma warning disable 0649
		// ReSharper disable All
		public enum PasswordAlg { sha512, sha256 }
		public enum PasswordFunc { pbkdf2 }

		public class APIResultAuthorize { public string pw_salt; public PasswordAlg pw_alg; public PasswordFunc pw_func; public int pw_cost, pw_key_size; }
		// ReSharper restore All
#pragma warning restore 0649

		private static WebClient CreateClient(IWebProxy proxy, string authToken)
		{
			var web = new WebClient();
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

		public static APIResultAuthorize Authenticate(IWebProxy proxy, string host, string mail, string password)
		{
			string result;

			try
			{
				var uri = CreateUri(host, "auth/params", "email=" + mail);

				result = CreateClient(proxy, null).DownloadString(uri);
			}
			catch (Exception e)
			{
				throw new StandardNoteAPIException("Communication with StandardNoteAPI failed", e);
			}

			try
			{
				var r = JsonConvert.DeserializeObject<APIResultAuthorize>(result);

				if (r.pw_func != PasswordFunc.pbkdf2) throw new Exception("Unknown pw_func: " + r.pw_func);

				byte[] bytes; 

				if (r.pw_alg == PasswordAlg.sha512)
				{
					using (var hmac = new HMACSHA256())
					{
						var df = new Pbkdf2(hmac, Encoding.UTF8.GetBytes(password), Convert.FromBase64String(r.pw_salt));
						bytes = df.GetBytes(r.pw_key_size / 64);
					}
				}
				else if (r.pw_alg == PasswordAlg.sha512)
				{
					using (var hmac = new HMACSHA256())
					{
						var df = new Pbkdf2(hmac, Encoding.UTF8.GetBytes(password), Convert.FromBase64String(r.pw_salt));
						bytes = df.GetBytes(r.pw_key_size / 64);
					}
				}
				else
				{
					throw new Exception("Unknown pw_alg: " + r.pw_alg);
				}
				
				var uri = CreateUri(host, "auth/sign-in");

				result = CreateClient(proxy, null).DownloadString(uri);

			}
			catch (Exception e)
			{
				throw new StandardNoteAPIException("Authentification with StandardNoteAPI failed.\r\nHTTP-Response:\r\n" + result, e);
			}
		}
		
	}
}
