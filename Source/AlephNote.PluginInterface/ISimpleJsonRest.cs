using System;

namespace AlephNote.PluginInterface
{
	public interface ISimpleJsonRest : IDisposable
	{
		void AddHeader(string name, string value);
		string GetResponseHeader(string name);
		void AddConverter(object c);  // we keep the type of ic object so not all plugins need to reference Json.Net, but typof(c) should be JsonConverter

		TResult PostTwoWay<TResult>(object body, string path, params string[] parameter);
		TResult PostTwoWay<TResult>(object body, string path, int[] allowedStatusCodes, params string[] parameter);

		void PostUpload(object body, string path, params string[] parameter);
		void PostUpload(object body, string path, int[] allowedStatusCodes, params string[] parameter);

		TResult PostDownload<TResult>(string path, params string[] parameter);
		TResult PostDownload<TResult>(string path, int[] allowedStatusCodes, params string[] parameter);

		TResult Get<TResult>(string path, params string[] parameter);
		TResult Get<TResult>(string path, int[] allowedStatusCodes, params string[] parameter);

		void Delete(object body, string path, params string[] parameter);
		void Delete(object body, string path, int[] allowedStatusCodes, params string[] parameter);

		void DeleteEmpty(string path, params string[] parameter);
		void DeleteEmpty(string path, int[] allowedStatusCodes, params string[] parameter);
	}
}