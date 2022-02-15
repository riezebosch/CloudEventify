namespace CloudEventify.MassTransit;

public interface ITypeMap
{
    /// <summary>
    /// Map the CLR type to the type name used in the cloud event envelope and vice versa.
    /// </summary>
    ITypeMap Map<T>(string type);
}