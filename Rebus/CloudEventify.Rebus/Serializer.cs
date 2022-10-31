using System.Text.Json;
using CloudNative.CloudEvents;
using Rebus.Bus;
using Rebus.Messages;
using Rebus.Serialization;

namespace CloudEventify.Rebus;

internal class Serializer : ISerializer
{
    private readonly CloudEventFormatter _formatter;
    private readonly JsonSerializerOptions _options;
    private readonly IMessageTypeNameConvention _convention;

    public Serializer(CloudEventFormatter formatter, JsonSerializerOptions options,
        IMessageTypeNameConvention convention)
    {
        _formatter = formatter;
        _options = options;
        _convention = convention;
    }

    Task<TransportMessage> ISerializer.Serialize(Message message) => 
        Task.FromResult(new TransportMessage(message.Headers, _formatter.Encode(new CloudEvent(CloudEventsSpecVersion.V1_0)
        {
            Id = message.GetMessageId(),
            Subject = null,
            Source = message.Headers.TryGetValue(Headers.SenderAddress, out var source) 
                ? new Uri(source, UriKind.Relative) 
                : new Uri("cloudeventify:rebus"),
            Data = message.Body,
            Time = DateTimeOffset.Parse(message.Headers[Headers.SentTime]),
            Type = message.Headers[Headers.Type]
        })));

    Task<Message> ISerializer.Deserialize(TransportMessage transportMessage)
    {
        var cloudEvent = _formatter.Decode(transportMessage.Body);
        var headers = new Dictionary<string, string>
        {
            [Headers.MessageId] = cloudEvent.Id!,
            [Headers.Type] = cloudEvent.Type!
        };

        return Task.FromResult(new Message(headers, ((JsonElement)cloudEvent.Data!).Deserialize(_convention.GetType(cloudEvent.Type!), _options)!));
    }
}