using System.IO;
using System.Net.Mime;
using CloudNative.CloudEvents;
using MassTransit;

namespace CloudEventify.MassTransit;

public class Serializer : IMessageSerializer
{
    private readonly CloudEventFormatter _formatter;
    private readonly Wrap _wrap;

    public Serializer(ContentType contentType, CloudEventFormatter formatter, Wrap wrap)
    {
        ContentType = contentType;
        _formatter = formatter;
        _wrap = wrap;
    }

    public void Serialize<T>(Stream stream, SendContext<T> context) where T : class => 
        stream.Write(_formatter.Encode(_wrap.Envelope(context)));

    public ContentType ContentType
    {
        get;
    }
}