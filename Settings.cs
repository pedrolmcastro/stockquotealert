namespace Settings;


using System.Net.Mail;
using System.Text.Json;
using System.Text.Json.Serialization;


internal sealed class MailAddressJsonConverter : JsonConverter<MailAddress>
{
    public override MailAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected a Json object to parse a MailAddress");
        }

        reader.Read();

        var address = ReadField("Address", ref reader);
        var display = ReadField("Display", ref reader);

        if (reader.TokenType != JsonTokenType.EndObject)
        {
            throw new JsonException("Missing Json object end");
        }

        return new MailAddress(address, display);
    }

    public override void Write(Utf8JsonWriter writer, MailAddress value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }


    private static string ReadField(string field, ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != field)
        {
            throw new JsonException($"Missing MailAddress Json property: {field}");
        }

        reader.Read();

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected MailAddress.{field} to be a string");
        }

        var value = reader.GetString();
        reader.Read();

        if (value is null)
        {
            throw new JsonException($"Failed to parse MailAddress.{field}");
        }

        return value;
    }
}


internal readonly record struct Smtp(
    Email.Host Host,
    Email.Credential Credential,
    MailAddress Sender,
    List<MailAddress> Receivers,
    uint Period
);

internal readonly record struct Api(uint Period);

internal readonly record struct Parsed(Smtp Smtp, Api Api);


internal static class Parser
{
    private const int ErrorCode = 2;


    public static Parsed Parse(string filepath)
    {
        if (!File.Exists(filepath))
        {
            Util.Error.Exit($"Missing settings file: {filepath}", ErrorCode);
        }

        var options = new JsonSerializerOptions()
        {
            Converters = { new MailAddressJsonConverter() } 
        };

        var parsed = new Parsed();

        try
        {
            var content = File.ReadAllText(filepath);
            parsed = JsonSerializer.Deserialize<Parsed>(content, options);
        }
        catch (Exception e)
        {
            Util.Error.Exit($"Failed to parse settings file: {e.Message}", ErrorCode);
        }

        return parsed;
    }
}
