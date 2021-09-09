using System.Buffers;
using System.Text.Json;

namespace MassTransit.CloudEvents
{
    /// <summary>
    /// https://github.com/dotnet/runtime/issues/31274#issuecomment-804360901
    /// </summary>
    internal static class JsonElementExtensions
    {
        public static T ToObject<T>(this JsonElement element, JsonSerializerOptions options = null)
        {
            var buffer = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(buffer))
            {
                element.WriteTo(writer);
            }

            return JsonSerializer.Deserialize<T>(buffer.WrittenSpan, options);
        }
    }
}
