namespace Game.Simulation.Features.Inventory.Contracts;

public sealed record ItemInstance(string InstanceId, string DefinitionId);
public sealed record EquippedItem(EquipmentSlot Slot, ItemInstance Item);

// The aggregate creates copied, read-only collections. Previous snapshots never change.
public sealed record InventoryState(IReadOnlyList<InventoryStack> Stacks,
    IReadOnlyList<ItemInstance> Items, IReadOnlyList<EquippedItem> Equipment);

public enum InventoryResult
{
    Success, UnknownDefinition, InvalidQuantity, NotStackable, NotIndividual,
    InsufficientQuantity, QuantityOverflow, InvalidInstanceId, DuplicateInstance,
    InstanceNotFound, AlreadyEquipped, InstanceEquipped, NotEquippable, InvalidSlot, SlotEmpty
}
