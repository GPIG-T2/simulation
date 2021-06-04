using System;
using System.Text.Json;

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
    }
}
