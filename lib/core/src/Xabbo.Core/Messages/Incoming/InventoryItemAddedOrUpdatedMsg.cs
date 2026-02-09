using Xabbo.Messages;
using Xabbo.Messages.Flash;

namespace Xabbo.Core.Messages.Incoming;

/// <summary>
/// Received when an item is added or updated in the user's inventory.
/// <para/>
/// Supported clients: <see cref="ClientType.Modern"/>
/// <para/>
/// Identifiers:
/// <list type="bullet">
/// <item>Flash: <see cref="In.FurniListAddOrUpdate"/></item>
/// </list>
/// </summary>
/// <param name="Items">The items that were added or updated.</param>
public sealed record InventoryItemAddedOrUpdatedMsg(InventoryItem[] Items)
    : IMessage<InventoryItemAddedOrUpdatedMsg>
{
    static ClientType IMessage<InventoryItemAddedOrUpdatedMsg>.SupportedClients => ClientType.Modern;
    static Identifier IMessage<InventoryItemAddedOrUpdatedMsg>.Identifier => In.FurniListAddOrUpdate;
    static InventoryItemAddedOrUpdatedMsg IParser<InventoryItemAddedOrUpdatedMsg>.Parse(in PacketReader p) => new(p.ParseArray<InventoryItem>());
    void IComposer.Compose(in PacketWriter p) => p.ComposeArray(Items);
}