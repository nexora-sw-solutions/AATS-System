using System.Text.Json;
using System.Text.Json.Serialization;

namespace AuditApp.API.Converters;

public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string Format = "yyyy-MM-dd";

    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            if (DateOnly.TryParse(value, out var date))
            {
                return date;
            }
            
            // Try parsing as DateTime if it's an ISO 8601 string with time
            if (DateTime.TryParse(value, out var dateTime))
            {
                return DateOnly.FromDateTime(dateTime);
            }
        }

        return DateOnly.FromDateTime(reader.GetDateTime());
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format));
    }
}
