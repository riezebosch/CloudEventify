using System.Text.Json;
using Rebus.Messages.Control;
using Rebus.Serialization;

namespace CloudEventify.Rebus;

internal class Builder : ICloudEvents
{
    private readonly ITypesMap _mapper = new TypesMapper()
        .Map<SubscribeRequest>("subscribe");

    private readonly JsonSerializerOptions _options = new();
    private Uri _source = new("cloudeventify:rebus");

    public ISerializer New() => 
        new Serializer(Formatter.New(_options), new Wrap(_mapper, _source), new Unwrap(_mapper, _options));

    ICloudEvents IConfigure<ICloudEvents>.WithJsonOptions(Action<JsonSerializerOptions> options)
    {
        options(_options);
        return this;
    }

    ICloudEvents ICloudEvents.WithSource(Uri source)
    {
        _source = source;
        return this;
    }

    ICloudEvents IConfigure<ICloudEvents>.WithTypes(Func<ITypesMap, ITypesMap> map)
    {
        map(_mapper);
        return this;
    }
}