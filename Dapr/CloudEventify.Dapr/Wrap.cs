using CloudNative.CloudEvents;

namespace CloudEventify.Dapr;

internal class Wrap
{
    private readonly ITypesMap _mapper;

    public Wrap(ITypesMap mapper) =>
        _mapper = mapper;

    public CloudEvent Envelope(object message) =>
        new(CloudEventsSpecVersion.V1_0)
        {
            Id = Guid.NewGuid().ToString(),
            Source = new Uri("cloudeventify:dapr"),
            Data = message,
            Time = DateTimeOffset.Now,
            Type = _mapper[message.GetType()]
        };
}