using MassTransit;

namespace CloudEventify.MassTransit;

public static class Factory
{
    public static ICloudEvents UseCloudEvents(this IBusFactoryConfigurator cfg)
    {
        var builder = new Builder();
        cfg.AddMessageDeserializer(builder.ContentType, () => builder.Deserializer());
        cfg.SetMessageSerializer(() => builder.Serializer());

        return builder;
    }

    public static ICloudEvents UseCloudEvents(this IReceiveEndpointConfigurator cfg)
    {
        var builder = new Builder();
        cfg.AddMessageDeserializer(builder.ContentType, () => builder.Deserializer());
        cfg.SetMessageSerializer(() => builder.Serializer());

        return builder;
    }
}