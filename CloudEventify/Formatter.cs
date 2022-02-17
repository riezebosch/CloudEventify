using System.Text.Json;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;

namespace CloudEventify;

public static class Formatter
{
    public static CloudEventFormatter New(JsonSerializerOptions options) => 
        new JsonEventFormatter(options, new JsonDocumentOptions());

    public static byte[] Encode(this CloudEventFormatter formatter, CloudEvent envelope) =>
        formatter.EncodeStructuredModeMessage(envelope, out _).ToArray();

    public static CloudEvent Decode(this CloudEventFormatter formatter, byte[] body) =>
        formatter.DecodeStructuredModeMessage(body, null, null);
}