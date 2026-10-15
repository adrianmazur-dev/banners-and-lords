using Game.Simulation.Features.Inventory.Contracts;

namespace Game.Simulation.Features.Inventory.Domain;

internal sealed class InventoryContents(IItemCatalog catalog)
{
    private readonly Dictionary<string, int> _stacks = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ItemInstance> _items = new(StringComparer.Ordinal);
    private readonly Dictionary<EquipmentSlot, string> _equipment = new();

    public InventoryState State { get; private set; } = new(
        Array.AsReadOnly<InventoryStack>([]), Array.AsReadOnly<ItemInstance>([]), Array.AsReadOnly<EquippedItem>([]));

    public int GetStackQuantity(string definitionId) => _stacks.GetValueOrDefault(definitionId);

    public InventoryResult ChangeStack(string definitionId, int quantity, bool adding)
    {
        if (!catalog.Contains(definitionId)) return InventoryResult.UnknownDefinition;
        if (catalog.GetDefinition(definitionId).Storage != ItemStorage.Stack) return InventoryResult.NotStackable;
        if (quantity <= 0) return InventoryResult.InvalidQuantity;
        var current = GetStackQuantity(definitionId);
        if (adding && quantity > int.MaxValue - current) return InventoryResult.QuantityOverflow;
        if (!adding && quantity > current) return InventoryResult.InsufficientQuantity;
        var next = adding ? current + quantity : current - quantity;
        if (next == 0) _stacks.Remove(definitionId); else _stacks[definitionId] = next;
        Refresh();
        return InventoryResult.Success;
    }

    public InventoryResult AddItem(ItemInstance item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (string.IsNullOrWhiteSpace(item.InstanceId)) return InventoryResult.InvalidInstanceId;
        if (!catalog.Contains(item.DefinitionId)) return InventoryResult.UnknownDefinition;
        if (catalog.GetDefinition(item.DefinitionId).Storage != ItemStorage.Individual) return InventoryResult.NotIndividual;
        if (!_items.TryAdd(item.InstanceId, item)) return InventoryResult.DuplicateInstance;
        Refresh();
        return InventoryResult.Success;
    }

    public InventoryResult RemoveItem(string instanceId)
    {
        if (string.IsNullOrWhiteSpace(instanceId) || !_items.ContainsKey(instanceId)) return InventoryResult.InstanceNotFound;
        if (_equipment.ContainsValue(instanceId)) return InventoryResult.InstanceEquipped;
        _items.Remove(instanceId);
        Refresh();
        return InventoryResult.Success;
    }

    public InventoryResult Equip(string instanceId)
    {
        if (string.IsNullOrWhiteSpace(instanceId) || !_items.TryGetValue(instanceId, out var item)) return InventoryResult.InstanceNotFound;
        if (_equipment.ContainsValue(instanceId)) return InventoryResult.AlreadyEquipped;
        var slot = catalog.GetDefinition(item.DefinitionId).Slot;
        if (slot is null) return InventoryResult.NotEquippable;
        // Ownership stays in one dictionary; replacing a slot releases the previous item to the bag.
        _equipment[slot.Value] = instanceId;
        Refresh();
        return InventoryResult.Success;
    }

    public InventoryResult Unequip(EquipmentSlot slot)
    {
        if (!Enum.IsDefined(slot)) return InventoryResult.InvalidSlot;
        if (!_equipment.Remove(slot)) return InventoryResult.SlotEmpty;
        Refresh();
        return InventoryResult.Success;
    }

    private void Refresh()
    {
        var worn = _equipment.Values.ToHashSet(StringComparer.Ordinal);
        State = new InventoryState(
            Array.AsReadOnly(_stacks.OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => new InventoryStack(pair.Key, pair.Value)).ToArray()),
            Array.AsReadOnly(_items.Values.Where(item => !worn.Contains(item.InstanceId))
                .OrderBy(item => item.InstanceId, StringComparer.Ordinal).ToArray()),
            Array.AsReadOnly(_equipment.OrderBy(pair => pair.Key)
                .Select(pair => new EquippedItem(pair.Key, _items[pair.Value])).ToArray()));
    }
}
