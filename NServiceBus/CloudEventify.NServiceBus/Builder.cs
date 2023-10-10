using System.Text.Json;
using NServiceBus.MessageInterfaces;
using NServiceBus.Serialization;
using NServiceBus.Settings;

namespace CloudEventify.NServiceBus;

public interface ICloudEvents : Types<ICloudEvents>, JsonOptions<ICloudEvents>;

/// <summary>
/// Builds the CloudEvents Serializer/Deserializer for NServiceBus.
/// Also implements required interfaces and abstractions to integrate with NServiceBus <see cref="SerializationDefinition"/>.
/// </summary>
public class Builder : SerializationDefinition, ICloudEvents
{
    private readonly ITryMap _mapper = new Mapper();
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Map explicit type names to .net types for serialization / deserialization
    /// </summary>
    /// <param name="map"></param>
    /// <returns></returns>
    public ICloudEvents WithTypes(Func<IMap, IMap> map)
    {
        map(_mapper);
        return this;
    }

    /// <summary>
    /// Configure specific JsonSerializerOptions for the CloudEvents Serializer/Deserializer
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public ICloudEvents WithJsonOptions(Action<JsonSerializerOptions> options)
    {
        options(_options);
        return this;
    }

    public override Func<IMessageMapper, IMessageSerializer> Configure(IReadOnlySettings settings)
    {
        return _ => new Serializer(_mapper, _options);
    }
}