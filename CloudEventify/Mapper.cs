namespace CloudEventify;

public class Mapper : IMap
{
    private readonly Dictionary<string, Type> _from = new();
    private readonly Dictionary<Type, TypeMap> _to = new();

    public IMap Map<T>(string typeName)
    {
        _from[typeName] = typeof(T);
        _to[typeof(T)] = new TypeMap(typeName, _ => null);
        return this;
    }
    
    public IMap Map<T>(string typeName, Func<TypeMap<T>, TypeMap<T>> configure)
    {
        var map = configure(new TypeMap<T>(typeName, _ => null));
        
        _from[map.Type] = typeof(T);
        _to[typeof(T)] = new TypeMap(map.Type, o => map.Subject((T)o));
        
        return this;
    }
    
    public Type this[string type] => _from[type];
    public TypeMap this[Type type] => _to[type];
}