using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Models
{
    public static class Json
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            IgnoreNullValues = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public static string Serialize<T>(T model) => JsonSerializer.Serialize(model, _jsonSerializerOptions);

        public static T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions);

        public static ValueTask<T?> DeserializeAsync<T>(Stream stream) => JsonSerializer.DeserializeAsync<T>(stream, _jsonSerializerOptions);
    }
}
