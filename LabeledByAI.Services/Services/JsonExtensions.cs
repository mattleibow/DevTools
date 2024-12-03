using System.Text.Json;
using System.Text.Json.Serialization;

namespace LabeledByAI.Services;

public static class JsonExtensions
{
    public static TObject? Deserialize<TObject>(this string? json) =>
        string.IsNullOrWhiteSpace(json)
            ? default
            : JsonSerializer.Deserialize<TObject>(json, SerializerOptions);

    public static string? Deserialize<TObject>(this TObject? obj) =>
        obj is null
            ? default
            : JsonSerializer.Serialize(obj, SerializerOptions);

    public static readonly JsonSerializerOptions SerializerOptions =
        new()
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
        };
}
