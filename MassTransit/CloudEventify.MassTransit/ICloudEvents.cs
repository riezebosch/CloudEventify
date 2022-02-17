using System.Net.Mime;

namespace CloudEventify.MassTransit;

public interface ICloudEvents : IConfigure<ICloudEvents>
{
    ICloudEvents WithContentType(ContentType contentType);
}