namespace CloudEventify;

public interface ITypesMap : IMap<Type, string>, IMap<string, Type>
{
    ITypesMap Map<T>(string type);
}