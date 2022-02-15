using MassTransit;

namespace CloudEventify.MassTransit;

public static class ConfiguratorExtensions
{
    public static IConfigurator UseCloudEvents(this IBusFactoryConfigurator cfg)
    {
        var deserializer = new Deserializer();
        cfg.AddMessageDeserializer(deserializer.ContentType, () => deserializer);

        var serializer = new Serializer();
        cfg.SetMessageSerializer(() => serializer);

        return new Configurator(serializer, deserializer);
    }

    public static IConfigurator UseCloudEvents(this IReceiveEndpointConfigurator cfg)
    {
        var deserializer = new Deserializer();
        cfg.AddMessageDeserializer(deserializer.ContentType, () => deserializer);

        var serializer = new Serializer();
        cfg.SetMessageSerializer(() => serializer);

        return new Configurator(serializer, deserializer);
    }
}