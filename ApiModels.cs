using System.Text.Json.Serialization;

namespace RestfulApiTests.Models;


/// <summary>
/// Represents the data payload of an object (nested properties like price, year, etc.)
/// </summary>
public class ObjectData
{
    [JsonPropertyName("year")]
    public string? Year { get; set; }

    [JsonPropertyName("price")]
    public string? Price { get; set; }

    [JsonPropertyName("CPU model")]
    public string? CpuModel { get; set; }

    [JsonPropertyName("Hard disk size")]
    public string? HardDiskSize { get; set; }

    [JsonPropertyName("color")]
    public string? Color { get; set; }

    [JsonPropertyName("capacity")]
    public string? Capacity { get; set; }

    [JsonPropertyName("screen size")]
    public double? ScreenSize { get; set; }

    [JsonPropertyName("Generation")]
    public string? Generation { get; set; }

    // Helper property to get price as a number (optional)
    [JsonIgnore]
    public double? PriceValue
    {
        get
        {
            if (string.IsNullOrEmpty(Price)) return null;
            if (double.TryParse(Price, out double result))
                return result;
            return null;
        }
    }
}

/// <summary>
/// Represents an API object returned by the restful-api.dev endpoints.
/// </summary>
public class ApiObject
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("data")]
    public ObjectData? Data { get; set; }

    // Store the Unix timestamp (milliseconds) from the API
    [JsonPropertyName("createdAt")]
    public long? CreatedAtUnix { get; set; }

    // Store the Unix timestamp (milliseconds) from the API
    [JsonPropertyName("updatedAt")]
    public long? UpdatedAtUnix { get; set; }

    [JsonIgnore]
    public DateTime? CreatedAt
    {
        get
        {
            if (CreatedAtUnix == null) return null;
            return DateTimeOffset.FromUnixTimeMilliseconds(CreatedAtUnix.Value).UtcDateTime;
        }
    }

    [JsonIgnore]
    public DateTime? UpdatedAt
    {
        get
        {
            if (UpdatedAtUnix == null) return null;
            return DateTimeOffset.FromUnixTimeMilliseconds(UpdatedAtUnix.Value).UtcDateTime;
        }
    }
}

// The request payload used when creating or updating an object.
public class CreateObjectRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public ObjectData? Data { get; set; }
}
