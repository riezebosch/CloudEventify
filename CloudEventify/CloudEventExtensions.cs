using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using CloudNative.CloudEvents;

namespace CloudEventify;

public static class CloudEventExtensions
{
    public static readonly JsonSerializerOptions CloudEventStandardJsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = {
            new JsonStringEnumConverter()
        }
    }; 

    public static CloudEvent DecodeToCloudEvent(this byte[] data) => Formatter.New(CloudEventStandardJsonSerializerOptions).Decode(data);
    public static CloudEvent DecodeToCloudEvent(this byte[] data, JsonSerializerOptions options) => Formatter.New(options).Decode(data);

    public static CloudEvent DecodeCloudEvent(this string message) => message.DecodeCloudEvent(CloudEventStandardJsonSerializerOptions);

    public static CloudEvent DecodeCloudEvent(this string message, JsonSerializerOptions options)
    {
        var cloudEventData = Encoding.ASCII.GetBytes(message);
        return cloudEventData.DecodeToCloudEvent(options);
    }

    public static string StandardEventTypeName(this Type dataType) =>
        (dataType.FullName!.Replace("Event", "", StringComparison.OrdinalIgnoreCase).Replace("Command", "", StringComparison.OrdinalIgnoreCase).ToKebabCase());
    
    public static string ToKebabCase(this string str) {
        // find and replace all parts that starts with one capital letter e.g. Asp, Net, Core
        var str1 = Regex.Replace(str, "[A-Z][a-z]+", m => $"-{m.ToString().ToLower()}", RegexOptions.None, TimeSpan.FromSeconds(5));

        // find and replace all parts that are all capital letter e.g. ASP, NET, CORE
        var str2 = Regex.Replace(str1, "[A-Z]+", m => $"-{m.ToString().ToLower()}", RegexOptions.None, TimeSpan.FromSeconds(5));

        return str2.TrimStart('-').ToLower();
    }

    public static byte[] ToByteArray(this CloudEvent cloudEvent, JsonSerializerOptions options) => Formatter.New(options).Encode(cloudEvent);

    public static byte[] ToByteArray(this CloudEvent cloudEvent) => cloudEvent.ToByteArray(CloudEventStandardJsonSerializerOptions);
}