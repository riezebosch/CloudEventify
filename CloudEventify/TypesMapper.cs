namespace CloudEventify;

public class TypesMapper : ITypesMap
{
    private readonly Dictionary<string, Type> _from = new();
    private readonly Dictionary<Type, ITypeMap> _to = new();

    public ITypesMap Map<T>(string typeName)
    {
        _from[typeName] = typeof(T);
        _to[typeof(T)] = new TypeMap(typeName);
        return this;
    }

    public ITypesMap WithFormatSubject<T>(Func<T, string> formatSubject)
    {
        _to[typeof(T)].WithFormatSubject(formatSubject);
        return this;
    }

    public Type this[string type] => _from[type];
    public ITypeMap this[Type type] => _to[type];
}