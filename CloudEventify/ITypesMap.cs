namespace CloudEventify;

public interface ITypesMap : IMap<Type, ITypeMap>, IMap<string, Type>
{
    ITypesMap Map<T>(string type);
    ITypesMap WithFormatSubject<T>(Func<T, string> formatSubject);
}

