using Dapr.Client;

namespace CloudEventify.Dapr;

internal class Builder : CloudEvents
{
    private readonly DaprClientBuilder _builder;
    private readonly ITypesMap _mapper = new TypesMapper();

    public Builder(DaprClientBuilder builder) => 
        _builder = builder;

    CloudEvents Types<CloudEvents>.WithTypes(Func<ITypesMap, ITypesMap> map)
    {
        map(_mapper);
        return this;
    }

    DaprClient CloudEvents.Build() =>
        new Client( _builder.Build(), new Wrap(_mapper));
}