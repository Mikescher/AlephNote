using AlephNote.PluginInterface;
using MSHC.Network;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;

namespace AlephNote.Repository
{
	public class SimpleJsonRest : ISimpleJsonRest
	{
		private const int LOG_FMT_DEPTH = 2;

		private readonly WebClient _client;
		private readonly Uri _host;
		private readonly IAlephLogger _logger;

		public SimpleJsonRest(IWebProxy proxy, string host, IAlephLogger log)
		{
			_client = new GZWebClient();
			_client.Headers["User-Agent"] = "AlephNote/" + App.APP_VERSION;
			if (proxy != null) _client.Proxy = proxy;
			_host = new Uri(host);

			_logger = log;
		}

		public void Dispose()
		{
			_client?.Dispose();
		}

		private Uri CreateUri(string path, params string[] parameter)
		{
			var uri = new Uri(_host, path);

			var result = uri.ToString();

			bool first = true;
			foreach (var param in parameter)
			{
				if (first)
					result += "?" + param;
				else
					result += "&" + param;

				first = false;
			}

			return new Uri(result);
		}

		public void AddHeader(string name, string value)
		{
			_client.Headers[name] = value;
		}

		public string GetResponseHeader(string name)
		{
			return _client.ResponseHeaders[name];
		}

		public TResult PostTwoWay<TResult>(object body, string path, params string[] parameter)
		{
			return PostTwoWay<TResult>(body, path, new int[0], parameter);
		}

		public TResult PostTwoWay<TResult>(object body, string path, int[] allowedStatusCodes, params string[] parameter)
		{
			var uri = CreateUri(path, parameter);

			string download;
			string upload;
			try
			{

				upload = JsonConvert.SerializeObject(body);

				download = _client.UploadString(uri, upload);
			}
			catch (WebException e)
			{
				var resp = e.Response as HttpWebResponse;
				if (resp != null)
				{
					if (allowedStatusCodes.Any(sc => sc == (int) resp.StatusCode))
					{
						_logger.Debug("REST", string.Format("REST call to '{0}' returned (allowed) statuscode {1} ({2})", uri, (int)resp.StatusCode, resp.StatusCode));
						return default(TResult);
					}

					throw new RestException("Server " + uri.Host + " returned status code: " + resp.StatusCode + " : " + resp.StatusDescription, e);
				}

				throw new RestException("Could not communicate with server " + uri.Host, e);
			}
			catch (Exception e)
			{
				throw new RestException("Could not communicate with server " + uri.Host, e);
			}

			TResult downloadObject;
			try
			{
				downloadObject = JsonConvert.DeserializeObject<TResult>(download);
			}
			catch (Exception e)
			{
				throw new RestException("Rest call to " + uri.Host + " returned unexpected data :\r\n" + download, e);
			}

			_logger.Debug("REST", 
				string.Format("Calling REST API '{0}' [POST]", uri), 
				string.Format("Send:\r\n{0}\r\n\r\n---------------------\r\n\r\nRecieved:\r\n{1}", 
				CompactJsonFormatter.FormatJSON(upload, LOG_FMT_DEPTH), 
				CompactJsonFormatter.FormatJSON(download, LOG_FMT_DEPTH)));

			return downloadObject;
		}

		public void PostUpload(object body, string path, params string[] parameter)
		{
			PostUpload(body, path, new int[0], parameter);
		}

		public void PostUpload(object body, string path, int[] allowedStatusCodes, params string[] parameter)
		{
			var uri = CreateUri(path, parameter);
			
			string upload;
			try
			{
				upload = JsonConvert.SerializeObject(body);

				_client.UploadString(uri, upload);
			}
			catch (WebException e)
			{
				var resp = e.Response as HttpWebResponse;
				if (resp != null)
				{
					if (allowedStatusCodes.Any(sc => sc == (int)resp.StatusCode))
					{
						_logger.Debug("REST", string.Format("REST call to '{0}' [POST] returned (allowed) statuscode {1} ({2})", uri, (int)resp.StatusCode, resp.StatusCode));
						return;
					}

					throw new RestException("Server " + uri.Host + " returned status code: " + resp.StatusCode + " : " + resp.StatusDescription, e);
				}

				throw new RestException("Could not communicate with server " + uri.Host, e);
			}
			catch (Exception e)
			{
				throw new RestException("Could not communicate with server " + uri.Host, e);
			}

			_logger.Debug("REST", 
				string.Format("Calling REST API '{0}' [POST]", uri), 
				string.Format("Send:\r\n{0}\r\n\r\nRecieved: Nothing",
				CompactJsonFormatter.FormatJSON(upload, LOG_FMT_DEPTH)));
		}

		public TResult PostDownload<TResult>(string path, params string[] parameter)
		{
			return PostDownload<TResult>(path, new int[0], parameter);
		}

		public TResult PostDownload<TResult>(string path, int[] allowedStatusCodes, params string[] parameter)
		{
			var uri = CreateUri(path, parameter);

			string download;
			try
			{
				download = _client.UploadString(uri, string.Empty);
			}
			catch (WebException e)
			{
				var resp = e.Response as HttpWebResponse;
				if (resp != null)
				{
					if (allowedStatusCodes.Any(sc => sc == (int)resp.StatusCode))
					{
						_logger.Debug("REST", string.Format("REST call to '{0}' [POST] returned (allowed) statuscode {1} ({2})", uri, (int)resp.StatusCode, resp.StatusCode));
						return default(TResult);
					}

					throw new RestException("Server " + uri.Host + " returned status code: " + resp.StatusCode + " : " + resp.StatusDescription, e);
				}

				throw new RestException("Could not communicate with server " + uri.Host, e);
			}
			catch (Exception e)
			{
				throw new RestException("Could not communicate with server " + uri.Host, e);
			}

			TResult downloadObject;
			try
			{
				downloadObject = JsonConvert.DeserializeObject<TResult>(download);
			}
			catch (Exception e)
			{
				throw new RestException("Rest call to " + uri.Host + " returned unexpected data :\r\n" + download, e);
			}

			_logger.Debug("REST", 
				string.Format("Calling REST API '{0}' [POST]", uri), 
				string.Format("Send: Nothing\r\nRecieved:\r\n{0}",
				CompactJsonFormatter.FormatJSON(download, LOG_FMT_DEPTH)));

			return downloadObject;
		}

		public TResult Get<TResult>(string path, params string[] parameter)
		{
			return Get<TResult>(path, new int[0], parameter);
		}

		public TResult Get<TResult>(string path, int[] allowedStatusCodes, params string[] parameter)
		{
			var uri = CreateUri(path, parameter);

			string download;
			try
			{
				download = _client.DownloadString(uri);
			}
			catch (WebException e)
			{
				var resp = e.Response as HttpWebResponse;
				if (resp != null)
				{
					if (allowedStatusCodes.Any(sc => sc == (int)resp.StatusCode))
					{
						_logger.Debug("REST", string.Format("REST call to '{0}' [GET] returned (allowed) statuscode {1} ({2})", uri, (int)resp.StatusCode, resp.StatusCode));
						return default(TResult);
					}

					throw new RestException("Server " + uri.Host + " returned status code: " + resp.StatusCode + " : " + resp.StatusDescription, e);
				}

				throw new RestException("Could not communicate with server " + uri.Host, e);
			}
			catch (Exception e)
			{
				throw new RestException("Could not communicate with server " + uri.Host, e);
			}

			TResult downloadObject;
			try
			{
				downloadObject = JsonConvert.DeserializeObject<TResult>(download);
			}
			catch (Exception e)
			{
				throw new RestException("Rest call to " + uri.Host + " returned unexpected data :\r\n" + download, e);
			}

			_logger.Debug("REST", 
				string.Format("Calling REST API '{0}' [GET]", uri), 
				string.Format("Send: Nothing\r\n\r\nRecieved:\r\n{0}",
				CompactJsonFormatter.FormatJSON(download, LOG_FMT_DEPTH)));

			return downloadObject;
		}

		public void Delete(object body, string path, params string[] parameter)
		{
			Delete(body, path, new int[0], parameter);
		}

		public void Delete(object body, string path, int[] allowedStatusCodes, params string[] parameter)
		{
			var uri = CreateUri(path, parameter);

			string upload;
			try
			{
				upload = JsonConvert.SerializeObject(body);

				_client.UploadString(uri, "DELETE", upload);
			}
			catch (WebException e)
			{
				var resp = e.Response as HttpWebResponse;
				if (resp != null)
				{
					if (allowedStatusCodes.Any(sc => sc == (int)resp.StatusCode))
					{
						_logger.Debug("REST", string.Format("REST call to '{0}' [DELETE] returned (allowed) statuscode {1} ({2})", uri, (int)resp.StatusCode, resp.StatusCode));
						return;
					}

					throw new RestException("Server " + uri.Host + " returned status code: " + resp.StatusCode + " : " + resp.StatusDescription, e);
				}

				throw new RestException("Could not communicate with server " + uri.Host, e);
			}
			catch (Exception e)
			{
				throw new RestException("Could not communicate with server " + uri.Host, e);
			}

			_logger.Debug("REST", 
				string.Format("Calling REST API '{0}' [DELETE]", uri), 
				string.Format("Send:\r\n{0}\r\n\r\nRecieved: Nothing",
				CompactJsonFormatter.FormatJSON(upload, LOG_FMT_DEPTH)));
		}

		public void DeleteEmpty(string path, params string[] parameter)
		{
			DeleteEmpty(path, new int[0], parameter);
		}

		public void DeleteEmpty(string path, int[] allowedStatusCodes, params string[] parameter)
		{
			var uri = CreateUri(path, parameter);
			
			try
			{
				_client.UploadString(uri, "DELETE", string.Empty);
			}
			catch (WebException e)
			{
				var resp = e.Response as HttpWebResponse;
				if (resp != null)
				{
					if (allowedStatusCodes.Any(sc => sc == (int)resp.StatusCode))
					{
						_logger.Debug("REST", string.Format("REST call to '{0}' [DELETE] returned (allowed) statuscode {1} ({2})", uri, (int)resp.StatusCode, resp.StatusCode));
						return;
					}

					throw new RestException("Server " + uri.Host + " returned status code: " + resp.StatusCode + " : " + resp.StatusDescription, e);
				}

				throw new RestException("Could not communicate with server " + uri.Host, e);
			}
			catch (Exception e)
			{
				throw new RestException("Could not communicate with server " + uri.Host, e);
			}

			_logger.Debug("REST", string.Format("Calling REST API '{0}' [DELETE]", uri), "Send: Nothing\r\n\r\nRecieved: Nothing");
		}
	}
}