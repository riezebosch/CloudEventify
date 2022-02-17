using System;
using System.Net.Mime;
using System.Text.Json;
using MassTransit;

namespace CloudEventify.MassTransit;

internal class Builder : ICloudEvents
{
    private readonly ITypesMap _mapper = new TypesMapper();
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };
    public ContentType ContentType { get; private set; } = new("application/cloudevents+json");


    ICloudEvents ICloudEvents.WithContentType(ContentType contentType)
    {
        ContentType = contentType;
        return this;
    }


    ICloudEvents IConfigure<ICloudEvents>.WithJsonOptions(Action<JsonSerializerOptions> options)
    {
        options(_options);
        return this;
    }

    ICloudEvents IConfigure<ICloudEvents>.WithTypes(Func<ITypesMap, ITypesMap> map)
    {
        map(_mapper);
        return this;
    }

    public IMessageDeserializer Deserializer() => 
        new Deserializer(ContentType, Formatter.New(_options), new Unwrap(_mapper, _options));

    public IMessageSerializer Serializer() => 
        new Serializer(ContentType, Formatter.New(_options), new Wrap(_mapper));
}