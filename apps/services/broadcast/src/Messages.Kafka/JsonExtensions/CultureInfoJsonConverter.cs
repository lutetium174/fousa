using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Messages.Kafka.JsonExtensions;

public class CultureInfoJsonConverter : JsonConverter<CultureInfo>
{
    public override CultureInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader switch
        {
            {TokenType: JsonTokenType.Null} => null!,
            {TokenType: JsonTokenType.String} when string.IsNullOrEmpty(reader.GetString()) => null!,
            {TokenType: JsonTokenType.String} => new(reader.GetString()!),
            
            _ => null!
        };

    public override void Write(Utf8JsonWriter writer, CultureInfo value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Name);
}