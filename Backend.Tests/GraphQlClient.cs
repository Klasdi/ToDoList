using System.Net.Http.Json;
using System.Text.Json;

namespace Backend.Tests;

public static class GraphQlClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static async Task<JsonDocument> PostAsync(HttpClient client, string query, object? variables = null, string? operationName = null)
    {
        var payload = new Dictionary<string, object?>
        {
            ["query"] = query,
            ["variables"] = variables,
            ["operationName"] = operationName
        };

        using var resp = await client.PostAsJsonAsync("/graphql", payload, Json);
        resp.EnsureSuccessStatusCode();
        var stream = await resp.Content.ReadAsStreamAsync();
        return await JsonDocument.ParseAsync(stream);
    }

    public static JsonElement? TryGetErrors(JsonDocument doc) =>
        doc.RootElement.TryGetProperty("errors", out var errors) ? errors : null;

    public static JsonElement GetData(JsonDocument doc) =>
        doc.RootElement.GetProperty("data");
}

