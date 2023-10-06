namespace CloudEventify;

public class Mapper : IMap, ITryMap
{
    private readonly Dictionary<string, Type> _from = new();
    private readonly Dictionary<Type, Map> _to = new();

    public IMap Map<T>(string typeName)
    {
        _from[typeName] = typeof(T);
        _to[typeof(T)] = new Map(typeName, _ => null);
        return this;
    }
    
    public IMap Map<T>(string typeName, Func<Map<T>, Map<T>> configure)
    {
        var map = configure(new Map<T>(typeName, _ => null));
        
        _from[map.Type] = typeof(T);
        _to[typeof(T)] = new Map(map.Type, o => map.Subject((T)o));
        
        return this;
    }
    
    public Type this[string type] => _from[type];
    public bool TryGet(string from, out Type? to) => _from.TryGetValue(from, out to);

    public Map this[Type type] =>  _to[type];
    public bool TryGet(Type from, out Map? to) => _to.TryGetValue(from, out to);
}