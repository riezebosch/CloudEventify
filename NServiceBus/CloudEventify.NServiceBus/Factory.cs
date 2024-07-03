namespace CloudEventify.NServiceBus;

public static class Factory
{
    /// <summary>
    /// Adds the CloudEvents Serializer/Deserializer to the NServiceBus.
    /// </summary>
    /// <param name="cfg">NServiceBus <see cref="EndpointConfiguration"/> to configure with CloudEvents Serializer/Deserializer</param>
    /// <returns>Itself to continue configuration fluently</returns>
    public static ICloudEvents UseCloudEvents(this EndpointConfiguration cfg)
    {
        var builder = new Builder();
        cfg.UseSerialization(builder);
        return builder;
    }
}