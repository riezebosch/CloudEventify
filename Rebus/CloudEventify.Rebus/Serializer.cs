using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using Rebus.Messages;
using Rebus.Serialization;

namespace CloudEventify.Rebus;

internal class Serializer : ISerializer
{
    Task<TransportMessage> ISerializer.Serialize(Message message)
    {
        var cloudEvent = new CloudEvent(CloudEventsSpecVersion.Default)
        {
            Data = message.Body,
            Source = new Uri("cloudeventify:rebus"),
            Id = message.Headers[Headers.MessageId],
            Type = message.Headers[Headers.Type],
            Time = DateTimeOffset.Parse(message.Headers[Headers.SentTime])
        };
        
        return Task.FromResult(new TransportMessage(message.Headers, cloudEvent.ToMessage()));
    }

    Task<Message> ISerializer.Deserialize(TransportMessage transportMessage)
    {
        var formatter = new JsonEventFormatter();
        var cloudEvent = formatter.DecodeStructuredModeMessage(transportMessage.Body, null, null);

        var headers = new Dictionary<string, string>
        {
            [Headers.MessageId] = cloudEvent.Id!
        };

        var body = cloudEvent.ToObject(Type.GetType(cloudEvent.Type));
        return Task.FromResult(new Message(headers, body));
    }
}