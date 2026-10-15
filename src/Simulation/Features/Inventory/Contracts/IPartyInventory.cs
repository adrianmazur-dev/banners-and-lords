namespace Game.Simulation.Features.Inventory.Contracts;

public interface IPartyInventory
{
    string PartyId { get; }
    IReadOnlyList<InventoryStack> Stacks { get; }
    int GetQuantity(string goodsId);
    bool Contains(string goodsId, int quantity);
    bool TryAdd(string goodsId, int quantity);
    bool TryRemove(string goodsId, int quantity);
}
