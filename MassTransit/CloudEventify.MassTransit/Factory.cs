using MassTransit;

namespace CloudEventify.MassTransit;

public static class Factory
{
    public static CloudEvents UseCloudEvents(this IBusFactoryConfigurator cfg)
    {
        var builder = new Builder();
        cfg.AddDeserializer(builder, true);
        cfg.AddSerializer(builder, true);

        return builder;
    }

    public static CloudEvents UseCloudEvents(this IReceiveEndpointConfigurator cfg)
    {
        var builder = new Builder();
        cfg.AddDeserializer(builder, true);
        cfg.AddSerializer(builder, true);

        return builder;
    }
}