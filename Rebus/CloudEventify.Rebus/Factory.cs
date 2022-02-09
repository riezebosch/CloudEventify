using Rebus.Config;
using Rebus.Serialization;

namespace CloudEventify.Rebus;

public static class Factory
{
    public static void UseCloudEvents(this StandardConfigurer<ISerializer> s) => 
        s.Register(r => new Serializer());
}