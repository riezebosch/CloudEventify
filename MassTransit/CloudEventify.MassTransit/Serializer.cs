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

    MessageBody IMessageSerializer.GetMessageBody<T>(SendContext<T> context) where T : class => 
        new ArrayMessageBody((byte[]?)_formatter.Encode(_wrap.Envelope(context)));

    public ContentType ContentType
    {
        get;
    }
}