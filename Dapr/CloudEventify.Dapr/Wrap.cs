using Dapr;

namespace CloudEventify.Dapr;

internal class Wrap
{
    private readonly ITypesMap _mapper;

    public Wrap(ITypesMap mapper) =>
        _mapper = mapper;

    public CloudEvent<T> Envelope<T>(T message) =>
        new(message)
        {
            Source = new Uri("cloudeventify:dapr"),
            Type = _mapper[message.GetType()].Type
        };
}