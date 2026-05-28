using Xabbo.Core;
using Xabbo.Web.Dto;

namespace Xabbo.Services.Abstractions;

public interface IHabboApi
{
    Task<MarketplaceResponse> FetchMarketplaceItemStats(Hotel hotel, ItemType type, string identifier, CancellationToken cancellationToken = default);
    Task<PhotoData> FetchPhotoDataAsync(Hotel hotel, string photoId, CancellationToken cancellationToken = default);
}
