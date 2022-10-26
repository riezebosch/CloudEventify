using System.Text.Json;
using CloudNative.CloudEvents;

namespace CloudEventify.MassTransit;

public class Unwrap
{
    private readonly JsonSerializerOptions _options;
    private readonly CloudEventify.Unwrap _unwrap;

    public Unwrap(IMap mapper, JsonSerializerOptions options)
    {
        _unwrap = new CloudEventify.Unwrap(mapper, options);
        _options = options;
    }

    public T Envelope<T>(CloudEvent cloudEvent) where T : class =>
        cloudEvent.Type!.StartsWith("MassTransit")
            ? ((JsonElement)cloudEvent.Data!).Deserialize<T>(_options)! // special treatment for MassTransit types, just use the target type
            : (T)_unwrap.Envelope(cloudEvent);
}