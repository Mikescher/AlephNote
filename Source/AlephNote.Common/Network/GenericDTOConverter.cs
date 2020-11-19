using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AlephNote.Common.Network
{
	class GenericDTOConverter : DateTimeConverterBase
	{
		private readonly Func<string, DateTimeOffset> _conv1;
		private readonly Func<DateTimeOffset, string> _conv2;

		public GenericDTOConverter(Func<string, DateTimeOffset> c1, Func<DateTimeOffset, string> c2)
		{
			_conv1 = c1;
			_conv2 = c2;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.Value is DateTimeOffset dto) return _conv1(dto.ToString("O"));
			if (reader.Value is DateTime dtv) return _conv1(dtv.ToString("O"));
			return _conv1(reader.Value.ToString());
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue(_conv2((DateTimeOffset)value));
		}
	}
}
