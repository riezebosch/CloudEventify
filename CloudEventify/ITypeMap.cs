namespace CloudEventify;

public interface ITypeMap
{
    string Type { get; }
    Func<object, string> Subject { get; }
    ITypeMap WithFormatSubject<T>(Func<T, string> format);
}