using Rebus.Config;
using Rebus.Pipeline;
using Rebus.Retry.Simple;
using Rebus.Serialization;

namespace CloudEventify.Rebus;

public static class Factory
{
    public static CloudEvents UseCloudEvents(this StandardConfigurer<ISerializer> s)
    {
        var builder = new Builder();
        s.Register(_ => builder.New());

        return builder;
    }

    /// <summary>
    /// Injects a pseudo random message id for incoming messages not published by rebus. 
    /// </summary>
    public static RebusConfigurer InjectMessageId(this RebusConfigurer configurer) =>
        configurer.Options(o => o.Decorate<IPipeline>(c =>
            new PipelineStepInjector(c.Get<IPipeline>()).OnReceive(new PseudoMessageIdStep(),
                PipelineRelativePosition.Before, typeof(SimpleRetryStrategyStep))));

}