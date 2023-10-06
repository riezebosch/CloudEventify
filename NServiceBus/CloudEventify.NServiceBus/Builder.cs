using System.Text.Json;
using NServiceBus.MessageInterfaces;
using NServiceBus.Serialization;
using NServiceBus.Settings;

namespace CloudEventify.NServiceBus;

public interface ICloudEvents : Types<ICloudEvents>, JsonOptions<ICloudEvents>;
public class Builder: SerializationDefinition, ICloudEvents
{
    private readonly ITryMap _mapper = new Mapper();
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };
    
    public override Func<IMessageMapper, IMessageSerializer> Configure(IReadOnlySettings settings)
    {
        return _ => new Serializer(_mapper, _options);
    }

    public ICloudEvents WithTypes(Func<IMap, IMap> map)
    {
        map(_mapper);
        return this;
    }

    public ICloudEvents WithJsonOptions(Action<JsonSerializerOptions> options)
    {
        options(_options);
        return this;
    }
}