namespace CloudEventify;

public interface Types<out T>
{
    /// <summary>
    /// Map CLR types to the type names used in the cloud event envelope and vice versa.
    /// See https://github.com/cloudevents/spec/blob/v1.0.2/cloudevents/spec.md#type
    /// </summary>
    T WithTypes(Func<IMap, IMap> map);
}