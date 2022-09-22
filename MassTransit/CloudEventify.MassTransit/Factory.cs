using MassTransit;

namespace CloudEventify.MassTransit;

public static class Factory
{
    public static CloudEvents UseCloudEvents(this IBusFactoryConfigurator cfg)
    {
        var builder = new Builder();
        cfg.AddMessageDeserializer(builder.ContentType, () => builder.Deserializer());
        cfg.SetMessageSerializer(() => builder.Serializer());

        return builder;
    }

    public static CloudEvents UseCloudEvents(this IReceiveEndpointConfigurator cfg)
    {
        var builder = new Builder();
        cfg.AddMessageDeserializer(builder.ContentType, () => builder.Deserializer());
        cfg.SetMessageSerializer(() => builder.Serializer());

        return builder;
    }
}