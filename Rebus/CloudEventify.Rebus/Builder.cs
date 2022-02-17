using System.Text.Json;
using Rebus.Messages.Control;
using Rebus.Serialization;

namespace CloudEventify.Rebus;

internal class Builder : ICloudEvents
{
    private readonly ITypesMap _mapper = new TypesMapper()
        .Map<SubscribeRequest>("subscribe");

    private readonly JsonSerializerOptions _options = new();
    public ISerializer New() => 
        new Serializer(Formatter.New(_options), new Wrap(_mapper), new Unwrap(_mapper, _options));

    ICloudEvents IConfigure<ICloudEvents>.WithJsonOptions(Action<JsonSerializerOptions> options)
    {
        options(_options);
        return this;
    }

    ICloudEvents IConfigure<ICloudEvents>.WithTypes(Func<ITypesMap, ITypesMap> map)
    {
        map(_mapper);
        return this;
    }
}