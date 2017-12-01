using AlephNote.PluginInterface;
using System;

namespace AlephNote.PluginInterface
{
	public class RestStatuscodeException : RestException
	{
		public readonly int StatusCode;
		public readonly string HTTPContent;
		public readonly string StatusPhrase;

		public RestStatuscodeException(string host, int statuscode, string phrase, string content)
			: base($"Server {host} returned status code: {statuscode} : {phrase}")
		{
			StatusCode = statuscode;
			HTTPContent = content;
			StatusPhrase = phrase;
		}
	}
}
