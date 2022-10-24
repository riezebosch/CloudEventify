using System;

namespace CloudEventify.MassTransit;

internal class TypesMapper : ITypesMap
{
    private readonly ITypesMap _map = new CloudEventify.TypesMapper();

    public ITypeMap this[Type type] => type.FullName!.StartsWith("MassTransit") 
        ? new TypeMap(type.FullName) // special treatment for MassTransit types, not explicitly mapped
        : _map[type];

    public Type this[string type] => _map[type];

    public ITypesMap Map<T>(string type) => 
        _map.Map<T>(type);

    public ITypesMap WithFormatSubject<T>(Func<T, string> formatSubject) => 
        _map.WithFormatSubject(formatSubject);
}