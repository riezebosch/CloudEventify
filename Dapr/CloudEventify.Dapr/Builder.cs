using Dapr.Client;

namespace CloudEventify.Dapr;

internal class Builder : CloudEvents
{
    private readonly DaprClientBuilder _builder;
    private readonly IMap _mapper = new Mapper();

    public Builder(DaprClientBuilder builder) => 
        _builder = builder;

    CloudEvents Types<CloudEvents>.WithTypes(Func<IMap, IMap> map)
    {
        map(_mapper);
        return this;
    }

    DaprClient CloudEvents.Build() =>
        new Client( _builder.Build(), new Wrap(_mapper));
}