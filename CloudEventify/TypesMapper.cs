namespace CloudEventify;

public class TypesMapper : ITypesMap
{
    private readonly Dictionary<string, Type> _from = new();
    private readonly Dictionary<Type, string> _to = new();

    public ITypesMap Map<T>(string type)
    {
        _from[type] = typeof(T);
        _to[typeof(T)] = type;
        
        return this;
    }

    public Type this[string type] => _from[type];
    public string this[Type type] => _to[type];
    public bool Has(Type type) => _to.ContainsKey(type);
    public bool Has(string type) => _from.ContainsKey(type);
}