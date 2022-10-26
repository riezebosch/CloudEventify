using Rebus.Config;
using Rebus.Serialization;
using Rebus.Transport;

namespace CloudEventify.Rebus;

public static class Factory
{
    public static CloudEvents UseCloudEvents(this StandardConfigurer<ISerializer> s)
    {
        var builder = new Builder();
        s.Register(_ => builder.New());

        return builder;
    }

    public static StandardConfigurer<ITransport> UseCloudEventAttributesForHeaders(this StandardConfigurer<ITransport> configurer)
    {
        configurer.Decorate(c => new CloudEventAttributeHeaderDecorator(c.Get<ITransport>()));
        return configurer;
    }
}
