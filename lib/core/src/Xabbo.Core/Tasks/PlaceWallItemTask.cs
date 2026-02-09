using System;
using Xabbo.Core.Messages.Incoming;
using Xabbo.Core.Messages.Outgoing;
using Xabbo.Interceptor;
using Xabbo.Interceptor.Tasks;

namespace Xabbo.Core.Tasks;

/// <summary>
/// Sends a request to place a wall item and returns whether the item was successfully placed.
/// </summary>
/// <param name="interceptor">The interceptor.</param>
/// <param name="itemId">The ID of the item to place.</param>
/// <param name="location">The location to place the item.</param>
[Intercept]
public sealed partial class PlaceWallItemTask(
    IInterceptor interceptor, Id itemId, WallLocation location
)
    : InterceptorTask<PlaceWallItemTask.Result>(interceptor)
{
    public enum Result { Error, Success }
    const string FurniPlacementError = "furni_placement_error";

    public Id ItemId { get; } = itemId;
    public WallLocation Location { get; } = location;

    protected override void OnExecute() => Interceptor.Send(new PlaceWallItemMsg(ItemId, Location));

    [Intercept]
    void HandleWallItemAdded(WallItemAddedMsg msg)
    {
        if (msg.Item.Id == Math.Abs(ItemId))
            SetResult(Result.Success);
    }

    // This is also received on Shockwave, but there is no mapping for the header.
    [Intercept]
    void HandleNotificationDialog(NotificationDialogMsg msg)
    {
        if (msg.Type == FurniPlacementError)
            SetResult(Result.Error);
    }

    // Receiving STRIPINFO on Shockwave indicates a failure to place an item.
    [Intercept(ClientType.Shockwave)]
    [InterceptIn(nameof(Xabbo.Messages.Shockwave.In.STRIPINFO))]
    void HandleStripInfo(Intercept e)
    {
        if (SetResult(Result.Error))
            e.Block();
    }
}