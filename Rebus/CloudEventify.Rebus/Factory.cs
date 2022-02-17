using Rebus.Config;
using Rebus.Serialization;

namespace CloudEventify.Rebus;

public static class Factory
{
    public static ICloudEvents UseCloudEvents(this StandardConfigurer<ISerializer> s)
    {
        var builder = new Builder();
        s.Register(_ => builder.New());

        return builder;
    }
}