using System;
using System.Buffers;
using System.Text.Json;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;

namespace CloudEventify.MassTransit;

public static class CloudEventExtensions
{
    public static T ToObject<T>(this CloudEvent element, Type type, JsonSerializerOptions? options = null)
    {
        // It is currently not possible to deserialize from a JsonElement: https://github.com/dotnet/runtime/issues/31274#issuecomment-804360901
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            ((JsonElement)element!.Data!).WriteTo(writer);
        }

        return (T)JsonSerializer.Deserialize(buffer.WrittenSpan, type, options)!;
    }

    public static ReadOnlyMemory<byte> ToMessage(this CloudEvent cloudEvent, JsonSerializerOptions? options = null)
    {
        var formatter = new JsonEventFormatter(options, new JsonDocumentOptions());
        return formatter.EncodeStructuredModeMessage(cloudEvent, out _);
    }
}