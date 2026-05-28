using System.Text.Json.Serialization;

namespace Xabbo.Web.Dto;

public sealed class MarketplaceResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = "";

    [JsonPropertyName("roomItemData")]
    public List<MarketplaceItemStats> RoomItemData { get; set; } = [];

    [JsonPropertyName("wallItemData")]
    public List<MarketplaceItemStats> WallItemData { get; set; } = [];
}
