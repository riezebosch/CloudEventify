using System.Text.Json;
using CloudNative.CloudEvents;
using Rebus.Bus;
using Rebus.Messages;
using Rebus.Serialization;

namespace CloudEventify.Rebus;

internal class Serializer : ISerializer
{
    private const string TraceParentExtensionAttribute = "traceparent";
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
        Task.FromResult(new TransportMessage(message.Headers, _formatter.Encode(ToCloudEvent(message))));

    Task<Message> ISerializer.Deserialize(TransportMessage transportMessage)
    {
        var cloudEvent = _formatter.Decode(transportMessage.Body);
        UpdateTransportHeaders(transportMessage, cloudEvent);

        return Task.FromResult(new Message(transportMessage.Headers, ((JsonElement)cloudEvent.Data!).Deserialize(_convention.GetType(cloudEvent.Type!), _options)!));
    }

    private static CloudEvent ToCloudEvent(Message message) =>
        new(CloudEventsSpecVersion.V1_0)
        {
            Id = message.GetMessageId(),
            Subject = message.GetMessageLabel(),
            Source = message.Headers.TryGetValue(Headers.SenderAddress, out var source)
                ? ToSource(source)
                : new Uri("cloudeventify:rebus"),
            Data = message.Body,
            Time = DateTimeOffset.Parse(message.Headers[Headers.SentTime]),
            Type = message.Headers[Headers.Type],
            [TraceParentExtensionAttribute] = message.Headers[Headers.CorrelationId]
        };

    /// <summary>
    /// A sender address is usually a queue name, which is a relative URI reference.
    /// An address set with UseSenderAddress may also be an absolute URI, such as a URN.
    /// The CloudEvents spec recommends an absolute URI for the source attribute.
    /// Every address that parses as relative is still read as relative.
    /// </summary>
    private static Uri ToSource(string address) =>
        Uri.TryCreate(address, UriKind.Relative, out var relative)
            ? relative
            : new Uri(address, UriKind.Absolute);

    private static void UpdateTransportHeaders(TransportMessage transportMessage, CloudEvent cloudEvent)
    {
        transportMessage.Headers[Headers.MessageId] = cloudEvent.Id!;
        transportMessage.Headers[Headers.Type] = cloudEvent.Type!;
        transportMessage.Headers[Headers.SenderAddress] = cloudEvent.Source!.ToString();
        transportMessage.Headers[Headers.CorrelationId] = (string?)cloudEvent[TraceParentExtensionAttribute];
    }
}