using Game.Simulation.Features.Inventory.Contracts;
using Game.Simulation.Features.Inventory.Domain;
using Game.Simulation.Features.Parties.Contracts;

namespace Game.Simulation.Features.Inventory.Application;

public sealed class PartyInventory : IPartyInventory
{
    private readonly IItemCatalog _catalog;
    private readonly InventoryContents _contents;

    public PartyInventory(PartyIdentity party, IItemCatalog catalog,
        IEnumerable<InventoryStack>? startingStacks = null, IEnumerable<ItemInstance>? startingItems = null)
    {
        ArgumentNullException.ThrowIfNull(party);
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentException.ThrowIfNullOrWhiteSpace(party.Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(party.CommanderId);
        PartyId = party.Id;
        CharacterId = party.CommanderId;
        _catalog = catalog;
        _contents = new InventoryContents(catalog);
        foreach (var stack in startingStacks ?? [])
            if (AddStack(stack.DefinitionId, stack.Quantity) != InventoryResult.Success)
                throw new ArgumentException($"Invalid starting stack '{stack.DefinitionId}'.", nameof(startingStacks));
        foreach (var item in startingItems ?? [])
            if (AddItem(item) != InventoryResult.Success)
                throw new ArgumentException($"Invalid starting item '{item.InstanceId}'.", nameof(startingItems));
    }

    public string PartyId { get; }
    public string CharacterId { get; }
    public InventoryState State => _contents.State;

    public int GetStackQuantity(string definitionId)
    {
        _catalog.GetDefinition(definitionId);
        return _contents.GetStackQuantity(definitionId);
    }

    public bool ContainsStack(string definitionId, int quantity) => quantity > 0 && _catalog.Contains(definitionId)
        && _contents.GetStackQuantity(definitionId) >= quantity;
    public InventoryResult AddStack(string definitionId, int quantity) => _contents.ChangeStack(definitionId, quantity, true);
    public InventoryResult RemoveStack(string definitionId, int quantity) => _contents.ChangeStack(definitionId, quantity, false);
    public InventoryResult AddItem(ItemInstance item) => _contents.AddItem(item);
    public InventoryResult RemoveItem(string instanceId) => _contents.RemoveItem(instanceId);
    public InventoryResult Equip(string instanceId) => _contents.Equip(instanceId);
    public InventoryResult Unequip(EquipmentSlot slot) => _contents.Unequip(slot);
}
