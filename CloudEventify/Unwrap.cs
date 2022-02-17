using System.Text.Json;
using CloudNative.CloudEvents;

namespace CloudEventify;

public class Unwrap
{
    private readonly IMap<string, Type> _map;
    private readonly JsonSerializerOptions _options;

    public Unwrap(IMap<string, Type> map, JsonSerializerOptions options)
    {
        _map = map;
        _options = options;
    }

    public object Envelope(CloudEvent envelope) => 
        ((JsonElement)envelope.Data!).Deserialize(_map[envelope.Type!], _options)!;
}