using System.Text.Json;
using Rebus.Config;
using Rebus.Pipeline;
using Rebus.Retry.Simple;
using Rebus.Serialization;
using Rebus.Serialization.Custom;
using Rebus.Topic;

namespace CloudEventify.Rebus;

public static class Factory
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="configurer"></param>
    /// <param name="options"></param>
    /// <returns>Use the <see cref="CustomTypeNameConventionBuilder"/> to map custom type names, either explicit with AddWithCustomName or implicit with AddWithShortName.</returns>
    public static CustomTypeNameConventionBuilder UseCloudEvents(this StandardConfigurer<ISerializer> configurer, JsonSerializerOptions? options = null)
    {
        options ??= new JsonSerializerOptions();
        configurer.Register(c => new Serializer(Formatter.New(options), options, c.Get<IMessageTypeNameConvention>()));
        return configurer.UseCustomMessageTypeNames();
    }

    /// <summary>
    /// Injects a pseudo random message id for incoming messages not published by rebus. 
    /// </summary>
    public static RebusConfigurer InjectMessageId(this RebusConfigurer configurer) => configurer.Options(o => 
        o.Decorate<IPipeline>(c => new PipelineStepInjector(c.Get<IPipeline>())
            .OnReceive(new PseudoMessageId(), PipelineRelativePosition.Before, typeof(SimpleRetryStrategyStep))));

    /// <summary>
    /// Use the configured custom type names for topic name and replaces "." with "/" to mimic nested resources.
    /// </summary>
    public static RebusConfigurer UseCustomTypeNameForTopicName(this RebusConfigurer configurer) => configurer
        .Options(o => o.Decorate<ITopicNameConvention>(c => new TopicNames(c.Get<IMessageTypeNameConvention>())));
}