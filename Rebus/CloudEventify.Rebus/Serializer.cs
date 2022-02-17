using CloudNative.CloudEvents;
using Rebus.Messages;
using Rebus.Serialization;

namespace CloudEventify.Rebus;

internal class Serializer : ISerializer
{
    private readonly CloudEventFormatter _formatter;
    private readonly Wrap _wrap;
    private readonly Unwrap _unwrap;

    public Serializer(CloudEventFormatter formatter, Wrap wrap, Unwrap unwrap)
    {
        _formatter = formatter;
        _wrap = wrap;
        _unwrap = unwrap;
    }

    Task<TransportMessage> ISerializer.Serialize(Message message) => 
        Task.FromResult(new TransportMessage(message.Headers, _formatter.Encode(_wrap.Envelope(message))));

    Task<Message> ISerializer.Deserialize(TransportMessage transportMessage)
    {
        var cloudEvent = _formatter.Decode(transportMessage.Body);
        var headers = new Dictionary<string, string>
        {
            [Headers.MessageId] = cloudEvent.Id!,
        };

        return Task.FromResult(new Message(headers, _unwrap.Envelope(cloudEvent)));
    }
}