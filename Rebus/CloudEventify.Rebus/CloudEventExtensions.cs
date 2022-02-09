using System.Buffers;
using System.Text.Json;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;

namespace CloudEventify.Rebus;

internal static class CloudEventExtensions
{
    public static object? ToObject(this CloudEvent element, Type type, JsonSerializerOptions? options = null)
    {
        // It is currently not possible to deserialize from a JsonElement: https://github.com/dotnet/runtime/issues/31274#issuecomment-804360901
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            ((JsonElement)element!.Data!).WriteTo(writer);
        }

        return JsonSerializer.Deserialize(buffer.WrittenSpan, type, options);
    }

    public static byte[] ToMessage(this CloudEvent cloudEvent, JsonSerializerOptions? options = null)
    {
        var formatter = new JsonEventFormatter(options, new JsonDocumentOptions());
        return formatter.EncodeStructuredModeMessage(cloudEvent, out _).ToArray();
    }
}