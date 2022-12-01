using System.Text.Json;
using Rebus.Config;
using Rebus.Pipeline;
using Rebus.Serialization;
using Rebus.Serialization.Custom;
using Rebus.Topic;

namespace CloudEventify.Rebus;

public static class Factory
{
    /// <summary>
    /// Use cloud events to wrap incoming and outgoing messages and be interoperable with the rest of the world.
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
    public static OptionsConfigurer InjectMessageId(this OptionsConfigurer options)
    {
        options
            .Decorate<IPipeline>(c => new PipelineStepConcatenator(c.Get<IPipeline>())
            .OnReceive(new PseudoMessageId(), PipelineAbsolutePosition.Front));
        return options;
    }

    /// <summary>
    /// Use the configured custom type names for topic name and replaces "." with "/" to mimic nested resources.
    /// </summary>
    public static OptionsConfigurer UseCustomTypeNameForTopicName(this OptionsConfigurer options)
    {
        options.Decorate<ITopicNameConvention>(c => new TopicNames(c.Get<IMessageTypeNameConvention>()));
        return options;
    }
}