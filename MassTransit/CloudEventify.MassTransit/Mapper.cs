using System;

namespace CloudEventify.MassTransit;

internal class Mapper : IMap
{
    private readonly IMap _map = new CloudEventify.Mapper();

    public Map this[Type type] => type.FullName!.StartsWith("MassTransit") 
        ? new Map(type.FullName, _ => null) // special treatment for MassTransit types, not explicitly mapped
        : _map[type];

    public Type this[string type] => _map[type];

    IMap IMap.Map<T>(string type) => 
        _map.Map<T>(type);

    IMap IMap.Map<T>(string type, Func<Map<T>, Map<T>> configure) =>
        _map.Map(type, configure);
}