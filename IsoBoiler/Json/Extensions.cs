using System.Text.Json;

namespace IsoBoiler.Json
{
    public static class Extensions
    {
        public static JsonSerializerOptions DefaultSerializerOptions = new JsonSerializerOptions() { WriteIndented = true, PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase};

        /// <summary>
        /// This extension method exists for simplicity and readability. It's basically a rename & passthrough of <see cref="Deserialize{TModel}(string, JsonSerializerOptions?)"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        public static T ToObject<T>(this string json, JsonSerializerOptions? jsonSerializerOptions = null)
        {
            return json.Deserialize<T>(jsonSerializerOptions);
        }

        /// <summary>
        /// This extension method exists for simplicity and readability. It's basically a rename & passthrough of <see cref="Serialize{TModel}(TModel, JsonSerializerOptions?)"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        public static string ToJson<TObject>(this TObject objectToSerialize, JsonSerializerOptions? jsonSerializerOptions = null)
        {
            return objectToSerialize.Serialize(jsonSerializerOptions);
        }

        public static TModel Deserialize<TModel>(this string stream, JsonSerializerOptions? jsonSerializerOptions)
        {
            return JsonSerializer.Deserialize<TModel>(stream, jsonSerializerOptions ?? DefaultSerializerOptions) ?? throw new JsonException("Failed to deserialize object");
        }
        public static TModel Deserialize<TModel>(this Stream stream, JsonSerializerOptions? jsonSerializerOptions)
        {
            return JsonSerializer.Deserialize<TModel>(stream, jsonSerializerOptions ?? DefaultSerializerOptions) ?? throw new JsonException("Failed to deserialize object");
        }

        public static string Serialize<TModel>(this TModel model, JsonSerializerOptions? jsonSerializerOptions)
        {
            return JsonSerializer.Serialize(model, jsonSerializerOptions ?? DefaultSerializerOptions);
        }
    }
}
