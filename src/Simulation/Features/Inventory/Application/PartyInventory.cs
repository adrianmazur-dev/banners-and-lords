using Game.Simulation.Features.Inventory.Contracts;
using Game.Simulation.Features.Inventory.Domain;
using Game.Simulation.Features.Parties.Contracts;

namespace Game.Simulation.Features.Inventory.Application;

public sealed class PartyInventory : IPartyInventory
{
    private readonly IGoodsCatalog _catalog;
    private readonly Cargo _cargo = new();

    public PartyInventory(PartyIdentity party, IGoodsCatalog catalog, IEnumerable<InventoryStack>? startingCargo = null)
    {
        ArgumentNullException.ThrowIfNull(party);
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentException.ThrowIfNullOrWhiteSpace(party.Id);
        PartyId = party.Id;
        _catalog = catalog;
        foreach (var stack in startingCargo ?? [])
        {
            if (!TryAdd(stack.GoodsId, stack.Quantity))
            {
                throw new ArgumentException($"Invalid starting cargo '{stack.GoodsId}'.", nameof(startingCargo));
            }
        }
    }

    public string PartyId { get; }
    public IReadOnlyList<InventoryStack> Stacks => _cargo.Stacks;

    public int GetQuantity(string goodsId)
    {
        _catalog.GetDefinition(goodsId);
        return _cargo.GetQuantity(goodsId);
    }

    public bool Contains(string goodsId, int quantity) => quantity > 0 && _catalog.Contains(goodsId)
        && _cargo.GetQuantity(goodsId) >= quantity;

    public bool TryAdd(string goodsId, int quantity) => _catalog.Contains(goodsId) && _cargo.TryAdd(goodsId, quantity);
    public bool TryRemove(string goodsId, int quantity) => _catalog.Contains(goodsId) && _cargo.TryRemove(goodsId, quantity);
}
