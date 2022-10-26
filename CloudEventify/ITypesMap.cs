namespace CloudEventify;

public interface IMap : ITypeMap<Type, TypeMap>, ITypeMap<string, Type>
{
    IMap Map<T>(string type);
    IMap Map<T>(string type, Func<TypeMap<T>, TypeMap<T>> configure);
}

