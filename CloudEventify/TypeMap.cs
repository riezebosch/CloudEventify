namespace CloudEventify;

public class TypeMap : ITypeMap
{
    public string Type { get; }
    public Func<object, string> Subject { get; private set; } = _ => default;

    public TypeMap(string type)
    {
        Type = type;
    }

    public ITypeMap WithFormatSubject<T>(Func<T, string> format)
    {
        Subject = obj => format((T)obj);
        return this;
    }
}