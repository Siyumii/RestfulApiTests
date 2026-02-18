using System.Net.Http.Json;
using System.Text.Json;
using RestfulApiTests.Models;

namespace RestfulApiTests.Helpers;

/// <summary>
/// A typed HTTP client wrapper for the restful-api.dev API.
/// Centralises base URL, serialisation settings, and common CRUD operations
/// so that test classes stay focused on assertions.
/// </summary>
public class ApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    private const string BaseUrl = "https://api.restful-api.dev";

    public ApiClient()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };

        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
    }

    // ── GET all objects ────────────────────────────────────────────────────────

    /// <summary>Returns every object.</summary>
    public async Task<List<ApiObject>?> GetAllObjectsAsync()
    {
        var response = await _httpClient.GetAsync("/objects");
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            var allow = response.Headers.Allow.Any() ? string.Join(", ", response.Headers.Allow) : "<none>";
            throw new HttpRequestException($"GET /objects returned {(int)response.StatusCode} {response.StatusCode}. Allow: {allow}. Body: {body}");
        }

        return await response.Content.ReadFromJsonAsync<List<ApiObject>>(_jsonOptions);
    }

    // ── GET single object ──────────────────────────────────────────────────────

    /// <summary>Returns the object with the given <paramref name="id"/>.</summary>
    public async Task<ApiObject?> GetObjectByIdAsync(string id)
    {
        var response = await _httpClient.GetAsync($"/objects/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ApiObject>(_jsonOptions);
    }

    /// <summary>
    /// Returns the raw <see cref="HttpResponseMessage"/> for a GET by id.
    /// </summary>
    public async Task<HttpResponseMessage> GetObjectByIdRawAsync(string id)
        => await _httpClient.GetAsync($"/objects/{id}");

    // ── POST (create) ──────────────────────────────────────────────────────────

    /// <summary>Creates a new object and returns the server response.</summary>
    public async Task<ApiObject?> CreateObjectAsync(CreateObjectRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/objects", request, _jsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ApiObject>(_jsonOptions);
    }

    // ── PUT (full update) ──────────────────────────────────────────────────────

    /// <summary>Fully replaces the object identified by <paramref name="id"/>.</summary>
    public async Task<ApiObject?> UpdateObjectAsync(string id, CreateObjectRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"/objects/{id}", request, _jsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ApiObject>(_jsonOptions);
    }

    // ── DELETE ─────────────────────────────────────────────────────────────────

    /// <summary>Deletes the object identified by <paramref name="id"/>.</summary>
    public async Task<HttpResponseMessage> DeleteObjectAsync(string id)
    {
        var response = await _httpClient.DeleteAsync($"/objects/{id}");
        response.EnsureSuccessStatusCode();
        return response;
    }

    // ── IDisposable ────────────────────────────────────────────────────────────

    public void Dispose() => _httpClient.Dispose();
}
