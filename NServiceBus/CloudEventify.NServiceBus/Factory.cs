namespace CloudEventify.NServiceBus;

public static class Factory
{
    public static ICloudEvents UseCloudEvents(this EndpointConfiguration cfg)
    {
        var builder = new Builder();
        cfg.UseSerialization(builder);
        return builder;
    }
}