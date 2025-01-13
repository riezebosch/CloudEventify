namespace CloudEventify;

public interface IMap : IMap<Type, Map>, IMap<string, Type>
{
    IMap Map<T>(string type);
    IMap Map<T>(string type, Func<Map<T>, Map<T>> configure);
}

public interface ITryMap : IMap, ITryMap<Type, Map>, ITryMap<string, Type>
{
}

