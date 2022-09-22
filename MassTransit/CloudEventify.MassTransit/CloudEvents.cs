using System.Net.Mime;

namespace CloudEventify.MassTransit;

public interface CloudEvents : Types<CloudEvents>, JsonOptions<CloudEvents>
{
    CloudEvents WithContentType(ContentType contentType);
}