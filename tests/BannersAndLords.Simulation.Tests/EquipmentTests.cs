using Game.Simulation.Features.Inventory.Application;
using Game.Simulation.Features.Inventory.Contracts;
using Game.Simulation.Features.Parties.Contracts;

namespace BannersAndLords.Simulation.Tests;

public sealed class EquipmentTests
{
    private static readonly PartyIdentity Party = new("test-party", "aldric");
    private static PartyInventory Create(params ItemInstance[] items) => new(Party, StartingInventory.CreateCatalog(),
        [new("bread", 60)], items);

    [Fact]
    public void IdenticalWeaponsRemainSeparateAndEquippingMovesOnlyTheChosenInstance()
    {
        var inventory = Create(new ItemInstance("first", "sword"), new("second", "sword"));
        Assert.Equal("aldric", inventory.CharacterId);
        Assert.Equal(InventoryResult.Success, inventory.Equip("second"));
        Assert.Equal(new ItemInstance("first", "sword"), Assert.Single(inventory.State.Items));
        Assert.Equal(new EquippedItem(EquipmentSlot.MainHand, new("second", "sword")), Assert.Single(inventory.State.Equipment));
        Assert.Equal(60, inventory.GetStackQuantity("bread"));
    }

    [Fact]
    public void ReplacingOccupiedSlotReturnsThePreviousInstanceAndUnequipRestoresBoth()
    {
        var inventory = Create(new ItemInstance("first", "sword"), new("knife", "dagger"), new("shield", "shield"));
        inventory.Equip("shield");
        inventory.Equip("first");
        var before = inventory.State;
        Assert.Equal(InventoryResult.Success, inventory.Equip("knife"));
        Assert.Equal(new ItemInstance("first", "sword"), Assert.Single(inventory.State.Items));
        Assert.Contains(new EquippedItem(EquipmentSlot.MainHand, new("knife", "dagger")), inventory.State.Equipment);
        Assert.Contains(new EquippedItem(EquipmentSlot.OffHand, new("shield", "shield")), inventory.State.Equipment);
        Assert.Contains(new EquippedItem(EquipmentSlot.MainHand, new("first", "sword")), before.Equipment);
        Assert.Equal(InventoryResult.Success, inventory.Unequip(EquipmentSlot.MainHand));
        Assert.Equal(2, inventory.State.Items.Count);
        Assert.Equal(EquipmentSlot.OffHand, Assert.Single(inventory.State.Equipment).Slot);
    }

    [Theory]
    [InlineData("helmet", EquipmentSlot.Head)]
    [InlineData("coat", EquipmentSlot.Body)]
    [InlineData("gloves", EquipmentSlot.Hands)]
    [InlineData("boots", EquipmentSlot.Feet)]
    [InlineData("sword", EquipmentSlot.MainHand)]
    [InlineData("dagger", EquipmentSlot.MainHand)]
    [InlineData("shield", EquipmentSlot.OffHand)]
    public void DefinitionsDetermineTheCompatibleSlot(string definitionId, EquipmentSlot expectedSlot)
    {
        var inventory = Create(new ItemInstance("instance", definitionId));
        Assert.Equal(InventoryResult.Success, inventory.Equip("instance"));
        Assert.Empty(inventory.State.Items);
        Assert.Equal(expectedSlot, Assert.Single(inventory.State.Equipment).Slot);
    }

    [Fact]
    public void DuplicateIdsAreRejectedAcrossCarriedAndWornItemsWithoutChangingState()
    {
        var inventory = Create(new ItemInstance("unique", "sword"));
        Assert.Equal(InventoryResult.DuplicateInstance, inventory.AddItem(new("unique", "dagger")));
        inventory.Equip("unique");
        var before = inventory.State;
        Assert.Equal(InventoryResult.DuplicateInstance, inventory.AddItem(new("unique", "helmet")));
        Assert.Same(before, inventory.State);
        Assert.Equal("sword", Assert.Single(inventory.State.Equipment).Item.DefinitionId);
    }

    [Fact]
    public void FailedCommandsLeaveTheEntireSnapshotUnchanged()
    {
        var inventory = Create(new ItemInstance("sword", "sword"));
        inventory.Equip("sword");
        var before = inventory.State;
        Assert.Equal(InventoryResult.AlreadyEquipped, inventory.Equip("sword"));
        Assert.Equal(InventoryResult.InstanceEquipped, inventory.RemoveItem("sword"));
        Assert.Equal(InventoryResult.InstanceNotFound, inventory.RemoveItem("missing"));
        Assert.Equal(InventoryResult.InstanceNotFound, inventory.Equip("missing"));
        Assert.Equal(InventoryResult.InstanceNotFound, inventory.Equip(" "));
        Assert.Equal(InventoryResult.SlotEmpty, inventory.Unequip(EquipmentSlot.Head));
        Assert.Equal(InventoryResult.InvalidSlot, inventory.Unequip((EquipmentSlot)99));
        Assert.Equal(InventoryResult.NotStackable, inventory.AddStack("sword", 1));
        Assert.Equal(InventoryResult.NotStackable, inventory.RemoveStack("sword", 1));
        Assert.Equal(InventoryResult.NotIndividual, inventory.AddItem(new("loaf", "bread")));
        Assert.Equal(InventoryResult.UnknownDefinition, inventory.AddItem(new("unknown", "missing")));
        Assert.Equal(InventoryResult.InvalidInstanceId, inventory.AddItem(new(" ", "sword")));
        Assert.Same(before, inventory.State);
    }

    [Fact]
    public void IndividualNonEquipmentCanBeCarriedButCannotBeEquipped()
    {
        var catalog = new ItemCatalog([new("relic", "Relic", ItemCategory.Goods, ItemStorage.Individual, "An old relic.")]);
        var inventory = new PartyInventory(Party, catalog, startingItems: [new("relic-1", "relic")]);
        var before = inventory.State;
        Assert.Equal(InventoryResult.NotEquippable, inventory.Equip("relic-1"));
        Assert.Same(before, inventory.State);
        Assert.Equal(InventoryResult.Success, inventory.RemoveItem("relic-1"));
        Assert.Empty(inventory.State.Items);
    }

    [Fact]
    public void EquipmentSnapshotsAreImmutableAndRetainPreviousLocations()
    {
        var source = new[] { new ItemInstance("first", "sword") };
        var inventory = Create(source);
        var carried = inventory.State;
        source[0] = new("changed", "helmet");
        Assert.Throws<NotSupportedException>(() => ((IList<ItemInstance>)carried.Items)[0] = source[0]);
        inventory.Equip("first");
        var worn = inventory.State;
        Assert.Throws<NotSupportedException>(() => ((IList<EquippedItem>)worn.Equipment).Clear());
        inventory.Unequip(EquipmentSlot.MainHand);
        inventory.RemoveItem("first");
        Assert.Equal("first", Assert.Single(carried.Items).InstanceId);
        Assert.Empty(carried.Equipment);
        Assert.Empty(worn.Items);
        Assert.Equal("first", Assert.Single(worn.Equipment).Item.InstanceId);
        Assert.Empty(inventory.State.Items);
    }

    [Fact]
    public void RepeatedSwapsAndRemovalsPreserveEveryRemainingInstanceExactlyOnce()
    {
        var inventory = StartingInventory.CreateFor(Party, StartingInventory.CreateCatalog());
        var original = inventory.State.Items.Select(item => item.InstanceId).Order().ToArray();
        for (var cycle = 0; cycle < 10; cycle++)
        {
            foreach (var id in original) Assert.Equal(InventoryResult.Success, inventory.Equip(id));
            foreach (var slot in Enum.GetValues<EquipmentSlot>())
                Assert.Equal(InventoryResult.Success, inventory.Unequip(slot));
            var owned = inventory.State.Items.Concat(inventory.State.Equipment.Select(entry => entry.Item))
                .Select(item => item.InstanceId).Order().ToArray();
            Assert.Equal(original, owned);
        }
        Assert.Equal(InventoryResult.Success, inventory.RemoveItem(original[0]));
        Assert.Equal(InventoryResult.Success, inventory.AddItem(new("replacement", "sword")));
        Assert.Equal(original.Length, inventory.State.Items.Count);
        Assert.DoesNotContain(inventory.State.Items, item => item.InstanceId == original[0]);
    }

    [Fact]
    public void EquipmentChangesAreIsolatedBetweenParties()
    {
        var catalog = StartingInventory.CreateCatalog();
        var first = StartingInventory.CreateFor(Party, catalog);
        var second = StartingInventory.CreateFor(new("other-party", "rowan"), catalog);
        var otherBefore = second.State;
        var sword = first.State.Items.First(item => item.DefinitionId == "sword");
        first.Equip(sword.InstanceId);
        first.Unequip(EquipmentSlot.MainHand);
        first.RemoveItem(sword.InstanceId);
        Assert.Same(otherBefore, second.State);
        Assert.Empty(second.State.Equipment);
        Assert.Equal(8, second.State.Items.Count);
        Assert.Equal(InventoryResult.InstanceNotFound, second.Equip(sword.InstanceId));
    }

    [Fact]
    public void InvalidInitialInstancesAndMissingCommanderAreRejected()
    {
        Assert.Throws<ArgumentException>(() => Create(new ItemInstance("same", "sword"), new("same", "helmet")));
        Assert.Throws<ArgumentException>(() => Create(new ItemInstance("unknown", "unknown")));
        Assert.Throws<ArgumentException>(() => Create(new ItemInstance("bread", "bread")));
        Assert.Throws<ArgumentException>(() => new PartyInventory(new("party", " "), StartingInventory.CreateCatalog()));
    }

    [Fact]
    public void CatalogRejectsStackableEquipmentAndInvalidSlotsOrStorage()
    {
        var sword = StartingInventory.CreateCatalog().GetDefinition("sword");
        Assert.Throws<ArgumentException>(() => new ItemCatalog([sword with { Storage = ItemStorage.Stack }]));
        Assert.Throws<ArgumentException>(() => new ItemCatalog([sword with { Slot = null }]));
        Assert.Throws<ArgumentException>(() => new ItemCatalog([sword with { Slot = (EquipmentSlot)99 }]));
        Assert.Throws<ArgumentException>(() => new ItemCatalog([sword with { Storage = (ItemStorage)99 }]));
        var bread = StartingInventory.CreateCatalog().GetDefinition("bread");
        Assert.Throws<ArgumentException>(() => new ItemCatalog([bread with { Slot = EquipmentSlot.Head }]));
    }
}
