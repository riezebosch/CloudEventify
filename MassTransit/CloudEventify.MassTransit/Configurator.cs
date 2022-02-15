using System;
using System.Net.Mime;
using System.Text.Json;

namespace CloudEventify.MassTransit;

internal class Configurator : IConfigurator, ITypeMap
{
    private readonly Serializer _serializer;
    private readonly Deserializer _deserializer;
    
    public Configurator(Serializer serializer, Deserializer deserializer)
    {
        _serializer = serializer;
        _deserializer = deserializer;
    }

    IConfigurator IConfigurator.WithContentType(ContentType contentType)
    {
        _serializer.ContentType =
            _deserializer.ContentType = contentType;
            
        return this;
    }

    IConfigurator IConfigurator.WithJsonOptions(Action<JsonSerializerOptions> options)
    {
        options(_serializer.Options);
        options(_deserializer.Options);
            
        return this;
    }

    IConfigurator IConfigurator.Type<T>(string type)
    {
        Map<T>(type);
        return this;
    }

    IConfigurator IConfigurator.WithTypes(Func<ITypeMap, ITypeMap> map)
    {
        map(this);
        return this;
    }

    public ITypeMap Map<T>(string type)
    {
        _serializer.AddType<T>(type);    
        _deserializer.AddType<T>(type);

        return this;
    }
}