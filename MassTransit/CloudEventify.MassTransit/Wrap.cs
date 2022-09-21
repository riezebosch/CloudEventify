using System;
using CloudNative.CloudEvents;
using MassTransit;

namespace CloudEventify.MassTransit;

public class Wrap
{
    private readonly ITypesMap _map;

    public Wrap(ITypesMap map) => 
        _map = map;

    public CloudEvent Envelope<T>(SendContext<T> context) where T : class =>
        new(CloudEventsSpecVersion.V1_0)
        {
            Id = context.MessageId.ToString(),
            Source = context.SourceAddress ?? new Uri("cloudeventify:masstransit"),
            Data = context.Message,
            Time = context.SentTime,
            Type = _map[typeof(T)].TypeName
        };
}