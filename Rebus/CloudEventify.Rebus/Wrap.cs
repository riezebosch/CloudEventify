using CloudNative.CloudEvents;
using Rebus.Bus;
using Rebus.Messages;

namespace CloudEventify.Rebus;

public class Wrap
{
    private readonly ITypesMap _mapper;
    private readonly Uri _source;

    public Wrap(ITypesMap mapper, Uri source)
    {
        _mapper = mapper;
        _source = source;
    }

    public CloudEvent Envelope(Message message) =>
        new(CloudEventsSpecVersion.V1_0)
        {
            Id = message.GetMessageId(),
            Subject = _mapper[message.Body.GetType()].FormatSubject(message.Body),
            Source = _source,
            Data = message.Body,
            Time = DateTimeOffset.Parse(message.Headers[Headers.SentTime]),
            Type = _mapper[message.Body.GetType()].TypeName
        };
}