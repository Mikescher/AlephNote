using System;

namespace AlephNote.PluginInterface
{
	public interface ISimpleJsonRest : IDisposable
	{
		void AddHeader(string name, string value);
		string GetResponseHeader(string name);

		void AddDTOConverter(Func<string, DateTimeOffset> c1, Func<DateTimeOffset, string> c2);
		void SetEscapeAllNonASCIICharacters(bool escape);
		void SetURLAuthentication(string username, string password);

		TResult PostTwoWay<TResult>(object body, string path, params string[] parameter);
		TResult PostTwoWay<TResult>(object body, string path, int[] allowedStatusCodes, params string[] parameter);
		void PostUpload(object body, string path, params string[] parameter);
		void PostUpload(object body, string path, int[] allowedStatusCodes, params string[] parameter);
		TResult PostDownload<TResult>(string path, params string[] parameter);
		TResult PostDownload<TResult>(string path, int[] allowedStatusCodes, params string[] parameter);
		void PostEmpty(string path, params string[] parameter);
		void PostEmpty(string path, int[] allowedStatusCodes, params string[] parameter);

		TResult PutTwoWay<TResult>(object body, string path, params string[] parameter);
		TResult PutTwoWay<TResult>(object body, string path, int[] allowedStatusCodes, params string[] parameter);
		void PutUpload(object body, string path, params string[] parameter);
		void PutUpload(object body, string path, int[] allowedStatusCodes, params string[] parameter);
		TResult PutDownload<TResult>(string path, params string[] parameter);
		TResult PutDownload<TResult>(string path, int[] allowedStatusCodes, params string[] parameter);
		void PutEmpty(string path, params string[] parameter);
		void PutEmpty(string path, int[] allowedStatusCodes, params string[] parameter);

		TResult DeleteTwoWay<TResult>(object body, string path, params string[] parameter);
		TResult DeleteTwoWay<TResult>(object body, string path, int[] allowedStatusCodes, params string[] parameter);
		void DeleteUpload(object body, string path, params string[] parameter);
		void DeleteUpload(object body, string path, int[] allowedStatusCodes, params string[] parameter);
		TResult DeleteDownload<TResult>(string path, params string[] parameter);
		TResult DeleteDownload<TResult>(string path, int[] allowedStatusCodes, params string[] parameter);
		void DeleteEmpty(string path, params string[] parameter);
		void DeleteEmpty(string path, int[] allowedStatusCodes, params string[] parameter);

		TResult Get<TResult>(string path, params string[] parameter);
		TResult Get<TResult>(string path, int[] allowedStatusCodes, params string[] parameter);

		TResult ParseJson<TResult>(string content);
		TResult ParseJsonWithoutConverter<TResult>(string content);
		TResult ParseJsonOrNull<TResult>(string content);

		string SerializeJson<TResult>(TResult obj);
	}
}