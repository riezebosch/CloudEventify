using CloudNative.CloudEvents;
using Rebus.Bus;
using Rebus.Messages;

namespace CloudEventify.Rebus;

public class Wrap
{
    private readonly IMap _mapper;
    private readonly Uri _source;

    public Wrap(IMap mapper, Uri source)
    {
        _mapper = mapper;
        _source = source;
    }

    public CloudEvent Envelope(Message message)
    {
        var map = _mapper[message.Body.GetType()];
        return new CloudEvent(CloudEventsSpecVersion.V1_0)
        {
            Id = message.GetMessageId(),
            Subject = map.Subject(message.Body),
            Source = _source,
            Data = message.Body,
            Time = DateTimeOffset.Parse(message.Headers[Headers.SentTime]),
            Type = map.Type
        };
    }
}