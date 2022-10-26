using System.Text.Json;
using Rebus.Messages.Control;
using Rebus.Serialization;

namespace CloudEventify.Rebus;

internal class Builder : CloudEvents
{
    private readonly IMap _mapper = new Mapper()
        .Map<SubscribeRequest>("subscribe");

    private readonly JsonSerializerOptions _options = new();
    private Uri _source = new("cloudeventify:rebus");

    public ISerializer New() => 
        new Serializer(Formatter.New(_options), new Wrap(_mapper, _source), new Unwrap(_mapper, _options));

    CloudEvents JsonOptions<CloudEvents>.WithJsonOptions(Action<JsonSerializerOptions> options)
    {
        options(_options);
        return this;
    }

    CloudEvents CloudEvents.WithSource(Uri source)
    {
        _source = source;
        return this;
    }

    CloudEvents Types<CloudEvents>.WithTypes(Func<IMap, IMap> map)
    {
        map(_mapper);
        return this;
    }
}