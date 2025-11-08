using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PickleBallBooking.API.Converters;

public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    private static readonly string[] Formats = { "HH:mm:ss", "HH:mm" };

    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            if (string.IsNullOrWhiteSpace(s))
                return default;
            foreach (var f in Formats)
            {
                if (TimeOnly.TryParseExact(s, f, out var t))
                    return t;
            }
            // Try default parse
            if (TimeOnly.TryParse(s, out var t2))
                return t2;
            throw new JsonException($"Invalid TimeOnly format: {s}. Expected HH:mm or HH:mm:ss");
        }
        throw new JsonException($"Unexpected token parsing TimeOnly. Expected String, got {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("HH:mm:ss"));
    }
}

public class NullableTimeOnlyJsonConverter : JsonConverter<TimeOnly?>
{
    private readonly TimeOnlyJsonConverter _inner = new();

    public override TimeOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;
        return _inner.Read(ref reader, typeof(TimeOnly), options);
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            _inner.Write(writer, value.Value, options);
        else
            writer.WriteNullValue();
    }
}
