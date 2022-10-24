using System;
using System.Net.Mime;
using System.Text.Json;
using MassTransit;

namespace CloudEventify.MassTransit;

internal class Builder : CloudEvents, ISerializerFactory
{
    private readonly ITypesMap _mapper = new TypesMapper();
    
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    IMessageSerializer ISerializerFactory.CreateSerializer() => 
        new Serializer(ContentType, Formatter.New(_options), new Wrap(_mapper));

    IMessageDeserializer ISerializerFactory.CreateDeserializer() => 
        new Deserializer(ContentType, Formatter.New(_options), new Unwrap(_mapper, _options), this);

    public ContentType ContentType { get; } = new("application/cloudevents+json");
    
    CloudEvents JsonOptions<CloudEvents>.WithJsonOptions(Action<JsonSerializerOptions> options)
    {
        options(_options);
        return this;
    }

    CloudEvents Types<CloudEvents>.WithTypes(Func<ITypesMap, ITypesMap> map)
    {
        map(_mapper);
        return this;
    }
}