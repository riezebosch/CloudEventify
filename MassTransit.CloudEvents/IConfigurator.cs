using System;
using System.Net.Mime;
using System.Text.Json;

namespace MassTransit.CloudEvents
{
    public interface IConfigurator
    {
        IConfigurator WithContentType(ContentType contentType);
        IConfigurator Type<T>(string type);
        IConfigurator WithJsonOptions(Action<JsonSerializerOptions> options);
    }
}