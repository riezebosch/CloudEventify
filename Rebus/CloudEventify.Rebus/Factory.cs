using Rebus.Config;
using Rebus.Serialization;
using Rebus.Transport;

namespace CloudEventify.Rebus;

public static class Factory
{
    public static RebusConfigurer UseCloudEvents(this RebusConfigurer rebusConfigurer, Action<CloudEvents> action)
    {
        return rebusConfigurer
            .Transport(c => c.Decorate(c => new TransportDecorator(c.Get<ITransport>())))
            .Serialization(s=> {
                var builder = new Builder();
                s.Register(_ => builder.New());

                action(builder);
            });
    }
}
