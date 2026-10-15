namespace Game.Simulation.Features.Inventory.Contracts;

public enum ItemCategory { Weapons, Armor, Food, Materials, Goods }
public enum ItemStorage { Stack, Individual }
public enum EquipmentSlot { Head, Body, Hands, Feet, MainHand, OffHand }

public sealed record ItemDefinition(string Id, string Name, ItemCategory Category,
    ItemStorage Storage, string Description, EquipmentSlot? Slot = null);
