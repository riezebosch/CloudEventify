using System.Text.Json;

namespace CloudEventify;

public interface IConfigure<out T>
{
    T WithJsonOptions(Action<JsonSerializerOptions> options);

    /// <summary>
    /// Map CLR types to the type names used in the cloud event envelope and vice versa.
    /// See https://github.com/cloudevents/spec/blob/v1.0.2/cloudevents/spec.md#type
    /// </summary>
    T WithTypes(Func<ITypesMap, ITypesMap> map);
}