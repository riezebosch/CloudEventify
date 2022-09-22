using System.Text.Json;

namespace CloudEventify;

public interface JsonOptions<out T>
{
    T WithJsonOptions(Action<JsonSerializerOptions> options);
}