using System.Text.Json;
using Rebus.Messages.Control;
using Rebus.Serialization;

namespace CloudEventify.Rebus;

internal class Builder : CloudEvents
{
    private readonly ITypesMap _mapper = new TypesMapper()
        .Map<SubscribeRequest>("subscribe");

    private readonly JsonSerializerOptions _options = new();
    public ISerializer New() => 
        new Serializer(Formatter.New(_options), new Wrap(_mapper), new Unwrap(_mapper, _options));

    CloudEvents JsonOptions<CloudEvents>.WithJsonOptions(Action<JsonSerializerOptions> options)
    {
        options(_options);
        return this;
    }

    CloudEvents Types<CloudEvents>.WithTypes(Func<ITypesMap, ITypesMap> map)
    {
        map(_mapper);
        return this;
    }
}