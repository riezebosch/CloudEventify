using System;
using CloudNative.CloudEvents;
using MassTransit;

namespace CloudEventify.MassTransit;

public class Wrap
{
    private readonly IMap _map;

    public Wrap(IMap map) => 
        _map = map;

    public CloudEvent Envelope<T>(SendContext<T> context) where T : class
    {
        var map = _map[typeof(T)];
        return new(CloudEventsSpecVersion.V1_0)
        {
            Id = context.MessageId.ToString(),
            Source = context.SourceAddress ?? new Uri("cloudeventify:masstransit"),
            Subject = map.Subject(context.Message),
            Data = context.Message,
            Time = context.SentTime,
            Type = map.Type
        };
    }
}