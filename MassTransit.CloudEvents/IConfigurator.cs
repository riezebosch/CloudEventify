using System.Net.Mime;

namespace MassTransit.CloudEvents
{
    public interface IConfigurator
    {
        IConfigurator WithContentType(ContentType contentType);
        IConfigurator Type<T>(string type);
    }
}