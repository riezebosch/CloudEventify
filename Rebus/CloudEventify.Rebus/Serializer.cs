using System.Runtime.CompilerServices;
using System.Text.Json;
using CloudNative.CloudEvents;
using Rebus.Bus;
using Rebus.Messages;
using Rebus.Serialization;

[assembly: InternalsVisibleTo("CloudEventify.Rebus.Tests")]
namespace CloudEventify.Rebus;

internal class Serializer : ISerializer
{
    private readonly CloudEventFormatter _formatter;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly IMessageTypeNameConvention _convention;
    private readonly Uri? _sourceAddress;

    public Serializer(CloudEventFormatter formatter, JsonSerializerOptions options,
        IMessageTypeNameConvention convention, Uri? sourceAddress = null)
    {
        _formatter = formatter;
        _jsonSerializerOptions = options;
        _convention = convention;
        _sourceAddress = sourceAddress;
    }

    Task<TransportMessage> ISerializer.Serialize(Message message) =>
        Task.FromResult(new TransportMessage(message.Headers, _formatter.Encode(new CloudEvent(CloudEventsSpecVersion.V1_0)
        {
            Id = message.GetMessageId(),
            Subject = message.GetMessageLabel(),
            Source = GetSourceAddress(message),
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

        return Task.FromResult(new Message(headers, ((JsonElement)cloudEvent.Data!).Deserialize(_convention.GetType(cloudEvent.Type!), _jsonSerializerOptions)!));
    }

    private Uri GetSourceAddress(Message message) => _sourceAddress ??
                                                        (message.Headers.TryGetValue(Headers.SenderAddress, out var source)
                                                            ? new Uri($"cloudeventify://rebus.queue.{source}")
                                                            : new Uri("cloudeventify://rebus"));
}