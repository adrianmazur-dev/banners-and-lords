namespace Game.Simulation.Features.Inventory.Contracts;

public interface IPartyInventory
{
    string PartyId { get; }
    string CharacterId { get; }
    InventoryState State { get; }
    int GetStackQuantity(string definitionId);
    bool ContainsStack(string definitionId, int quantity);
    InventoryResult AddStack(string definitionId, int quantity);
    InventoryResult RemoveStack(string definitionId, int quantity);
    InventoryResult AddItem(ItemInstance item);
    InventoryResult RemoveItem(string instanceId);
    InventoryResult Equip(string instanceId);
    InventoryResult Unequip(EquipmentSlot slot);
}
