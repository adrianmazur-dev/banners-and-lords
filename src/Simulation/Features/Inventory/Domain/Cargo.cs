using Game.Simulation.Features.Inventory.Contracts;

namespace Game.Simulation.Features.Inventory.Domain;

internal sealed class Cargo
{
    private readonly Dictionary<string, int> _quantities = new(StringComparer.Ordinal);

    public IReadOnlyList<InventoryStack> Stacks { get; private set; } = Array.AsReadOnly<InventoryStack>([]);
    public int GetQuantity(string goodsId) => _quantities.GetValueOrDefault(goodsId);

    public bool TryAdd(string goodsId, int quantity)
    {
        var current = GetQuantity(goodsId);
        if (quantity <= 0 || quantity > int.MaxValue - current)
        {
            return false;
        }
        _quantities[goodsId] = current + quantity;
        RefreshSnapshot();
        return true;
    }

    public bool TryRemove(string goodsId, int quantity)
    {
        var current = GetQuantity(goodsId);
        if (quantity <= 0 || current < quantity)
        {
            return false;
        }
        if (current == quantity)
        {
            _quantities.Remove(goodsId);
        }
        else
        {
            _quantities[goodsId] = current - quantity;
        }
        RefreshSnapshot();
        return true;
    }

    private void RefreshSnapshot() => Stacks = Array.AsReadOnly(_quantities
        .OrderBy(entry => entry.Key, StringComparer.Ordinal)
        .Select(entry => new InventoryStack(entry.Key, entry.Value))
        .ToArray());
}
