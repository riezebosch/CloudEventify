namespace CloudEventify;

public class TypeMap : ITypeMap
{
    public string TypeName { get; private set; }
    public Func<object, string> FormatSubject { get; private set; } = (a) => default;

    public TypeMap(string typeName)
    {
        TypeName = typeName;
    }

    public ITypeMap WithFormatSubject<T>(Func<T, string> formatSubject)
    {
        FormatSubject = new Func<object, string>(obj => formatSubject((T)obj));
        return this;
    }
}

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
    public bool Has(Type type) => _to.ContainsKey(type);
    public bool Has(string type) => _from.ContainsKey(type);
}