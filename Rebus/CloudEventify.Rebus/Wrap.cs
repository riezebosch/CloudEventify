using CloudNative.CloudEvents;
using Rebus.Bus;
using Rebus.Messages;

namespace CloudEventify.Rebus;

public class Wrap
{
    private readonly ITypesMap _mapper;

    public Wrap(ITypesMap mapper) =>
        _mapper = mapper;

    public CloudEvent Envelope(Message message) =>
        new(CloudEventsSpecVersion.V1_0)
        {
            Id = message.GetMessageId(),
            Source = new Uri("cloudeventify:rebus"),
            Data = message.Body,
            Time = DateTimeOffset.Parse(message.Headers[Headers.SentTime]),
            Type = _mapper[message.Body.GetType()]
        };
}