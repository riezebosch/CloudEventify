using System.Text.Json;

using Rebus.Config;
using Rebus.Pipeline;
using Rebus.Serialization;
using Rebus.Serialization.Custom;
using Rebus.Topic;

namespace CloudEventify.Rebus;

public class CloudEventsOptions
{
    private readonly RebusConfigurer _configurer;
    internal JsonSerializerOptions JsonSerializerOptions = new();

    public CloudEventsOptions(RebusConfigurer configurer)
    {
        _configurer = configurer;
    }

    public CloudEventsOptions Configure(Action<JsonSerializerOptions> configure)
    {
        if(configure is null)
            throw new ArgumentNullException(nameof(configure));

        configure.Invoke(JsonSerializerOptions);
        return this;
    }

    public CloudEventsOptions With(JsonSerializerOptions serializerOptions)
    {
        JsonSerializerOptions = serializerOptions;
        return this;
    }

    public CloudEventsOptions UseCustomTypeNameForTopicName()
    {
        _configurer.Options(options => options.Decorate<ITopicNameConvention>(c => new TopicNames(c.Get<IMessageTypeNameConvention>())));
        return this;
    }

    public CloudEventsOptions InjectMessageId()
    {
        _configurer.Options(options => options.Decorate<IPipeline>(c => new PipelineStepConcatenator(c.Get<IPipeline>())
                                                                       .OnReceive(new PseudoMessageId(), PipelineAbsolutePosition.Front)));
        return this;
    }

    private readonly List<Action<CustomTypeNameConventionBuilder>> _typeRegistrations = new();

    public CloudEventsOptions RegisterTypeWithCustomName<T>(string topic) => RegisterType(builder => builder.AddWithCustomName<T>(topic));
    public CloudEventsOptions RegisterTypeWithCustomName(Type type, string topic) => RegisterType(builder => builder.AddWithCustomName(type, topic));
    public CloudEventsOptions RegisterTypeWithShortName(Type type) => RegisterType(builder => builder.AddWithShortName(type));
    public CloudEventsOptions RegisterTypeWithShortName<T>() => RegisterType(builder => builder.AddWithShortName<T>());

    public CloudEventsOptions RegisterType(Action<CustomTypeNameConventionBuilder> registration)
    {
        if(registration is null) throw new ArgumentNullException(nameof(registration));

        _typeRegistrations.Add(registration);
        return this;
    }

    public RebusConfigurer Configure()
    {
        return _configurer.Serialization(config =>
                                         {
                                             config.Register(c => new Serializer(Formatter.New(JsonSerializerOptions),
                                                                                 JsonSerializerOptions,
                                                                                 c.Get<IMessageTypeNameConvention>()));
                                             var x = config.UseCustomMessageTypeNames();
                                             foreach (var registration in _typeRegistrations)
                                             {
                                                 registration.Invoke(x);
                                             }
                                         });
    }
}