using Rebus.Serialization;
using Rebus.Topic;

namespace CloudEventify.Rebus;

internal class TopicNames : ITopicNameConvention
{
    private readonly IMessageTypeNameConvention _convention;

    public TopicNames(IMessageTypeNameConvention convention) => 
        _convention = convention;

    public string GetTopic(Type eventType) => 
        _convention.GetTypeName(eventType).Replace(".", "/");
}