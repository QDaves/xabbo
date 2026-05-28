using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Xabbo.Core;
using Xabbo.Services.Abstractions;
using Xabbo.Web.Serialization;

namespace Xabbo.Services;

public sealed class HabboApi : IHabboApi
{
    private readonly HttpClient _http = new()
    {
        DefaultRequestHeaders = {
            { "User-Agent", "xabbo" }
        }
    };

    private async Task<T> GetRequiredDataAsync<T>(Hotel hotel, string path, CancellationToken cancellationToken = default)
    {
        if (!path.StartsWith('/'))
            throw new ArgumentException("Path must start with '/'.", nameof(path));

        var typeInfo = JsonWebContext.Default.GetTypeInfo(typeof(T)) as JsonTypeInfo<T>
            ?? throw new Exception($"Failed to get type info for '{typeof(T)}'.");

        var res = await _http.GetAsync($"https://{hotel.WebHost}{path}", cancellationToken);
        res.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<T>(
            res.Content.ReadAsStream(cancellationToken), typeInfo, cancellationToken)
            ?? throw new Exception($"Failed to deserialize {typeInfo.Type.Name}.");
    }

    private async Task<T> PostRequiredDataAsync<T>(Hotel hotel, string path, HttpContent content, CancellationToken cancellationToken = default)
    {
        if (!path.StartsWith('/'))
            throw new ArgumentException("Path must start with '/'.", nameof(path));

        var typeInfo = JsonWebContext.Default.GetTypeInfo(typeof(T)) as JsonTypeInfo<T>
            ?? throw new Exception($"Failed to get type info for '{typeof(T)}'.");

        var res = await _http.PostAsync($"https://{hotel.WebHost}{path}", content, cancellationToken);
        res.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<T>(
            res.Content.ReadAsStream(cancellationToken), typeInfo, cancellationToken)
            ?? throw new Exception($"Failed to deserialize {typeInfo.Type.Name}.");
    }

    public Task<Web.Dto.MarketplaceResponse> FetchMarketplaceItemStats(Hotel hotel, ItemType type, string identifier, CancellationToken cancellationToken = default)
    {
        string? typeString = type switch
        {
            ItemType.Floor => "roomItems",
            ItemType.Wall => "wallItems",
            _ => throw new Exception($"Invalid item type: {type}.")
        };

        var payload = new Dictionary<string, object>
        {
            [typeString] = new[]
            {
                new { item = identifier }
            }
        };

        var json = JsonSerializer.Serialize(payload, JsonWebContext.Default.Options);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        return PostRequiredDataAsync<Web.Dto.MarketplaceResponse>(
            hotel, $"/api/public/marketplace/stats/batch/", content, cancellationToken);
    }

    public Task<Web.Dto.PhotoData> FetchPhotoDataAsync(Hotel hotel, string photoId, CancellationToken cancellationToken = default)
    {
        return GetRequiredDataAsync<Web.Dto.PhotoData>(
            hotel, $"/photodata/public/furni/{photoId}", cancellationToken);
    }
}
