namespace CloudEventify;

public interface ITypesMap : IMap<Type, ITypeMap>, IMap<string, Type>
{
    ITypesMap Map<T>(string type);
    ITypesMap WithFormatSubject<T>(Func<T, string> formatSubject);
}

public interface ITypeMap
{
    string TypeName { get; }
    Func<object, string> FormatSubject { get; }
    ITypeMap WithFormatSubject<T>(Func<T, string> formatSubject);
}