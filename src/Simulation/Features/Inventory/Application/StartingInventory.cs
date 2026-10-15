using Game.Simulation.Features.Inventory.Contracts;
using Game.Simulation.Features.Parties.Contracts;

namespace Game.Simulation.Features.Inventory.Application;

public static class StartingInventory
{
    public static IItemCatalog CreateCatalog() => new ItemCatalog(
    [
        new("sword", "Arming sword", ItemCategory.Weapons, ItemStorage.Individual,
            "A straight, one-handed blade with a leather-wrapped grip.", EquipmentSlot.MainHand),
        new("dagger", "Belt knife", ItemCategory.Weapons, ItemStorage.Individual,
            "A short blade suited to a traveler's belt.", EquipmentSlot.MainHand),
        new("shield", "Painted shield", ItemCategory.Armor, ItemStorage.Individual,
            "A wooden shield bearing a faded red and gold cross.", EquipmentSlot.OffHand),
        new("helmet", "Iron helmet", ItemCategory.Armor, ItemStorage.Individual,
            "A simple iron helmet with a reinforced nose guard.", EquipmentSlot.Head),
        new("coat", "Padded coat", ItemCategory.Armor, ItemStorage.Individual,
            "Layers of quilted cloth offer protection beneath armor.", EquipmentSlot.Body),
        new("gloves", "Leather gloves", ItemCategory.Armor, ItemStorage.Individual,
            "Supple leather gloves protect the hands.", EquipmentSlot.Hands),
        new("boots", "Leather boots", ItemCategory.Armor, ItemStorage.Individual,
            "Sturdy boots made for the roads of the Alder Valley.", EquipmentSlot.Feet),
        new("bread", "Bread", ItemCategory.Food, ItemStorage.Stack, "Loaves carried together as a single stack of provisions."),
        new("grain", "Grain", ItemCategory.Food, ItemStorage.Stack, "Sacks of grain for provisions and trade."),
        new("fish", "Fish", ItemCategory.Food, ItemStorage.Stack, "Fish packed among the party's provisions."),
        new("timber", "Timber", ItemCategory.Materials, ItemStorage.Stack, "Cut timber for building, deliveries, and trade."),
        new("tools", "Tools", ItemCategory.Goods, ItemStorage.Stack, "Hand tools for work in the field and at home.")
    ]);

    public static IPartyInventory CreateFor(PartyIdentity party, IItemCatalog catalog) => new PartyInventory(party, catalog,
        [new("bread", 60), new("grain", 20), new("fish", 8), new("timber", 20), new("tools", 2)],
        new[] { "sword", "sword", "dagger", "shield", "helmet", "coat", "gloves", "boots" }
            .Select((id, index) => new ItemInstance($"{party.Id}-item-{index + 1:D2}", id)));
}
