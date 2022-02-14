using System;
using System.Net.Mime;
using System.Text.Json;

namespace MassTransit.CloudEvents;

public interface IConfigurator
{
    IConfigurator WithContentType(ContentType contentType);
    IConfigurator WithJsonOptions(Action<JsonSerializerOptions> options);
    [Obsolete("use WithTypes(t => t.Map<T>(string)) instead")]
    IConfigurator Type<T>(string type);

    /// <summary>
    /// Map CLR types to the type names used in the cloud event envelope and vice versa.
    /// </summary>
    IConfigurator WithTypes(Func<ITypeMap, ITypeMap> map);
}