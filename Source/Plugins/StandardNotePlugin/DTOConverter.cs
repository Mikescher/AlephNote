using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace AlephNote.Plugins.StandardNote
{
	class DTOConverter : DateTimeConverterBase
	{
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			DateTimeOffset o;
			if (DateTimeOffset.TryParse(reader.Value.ToString(), out o)) return o;
			return DateTimeOffset.MinValue;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue(((DateTimeOffset)value).UtcDateTime.ToString("O"));
		}
	}
}
