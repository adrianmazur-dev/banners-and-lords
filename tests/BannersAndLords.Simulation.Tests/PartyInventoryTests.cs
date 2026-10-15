using Game.Simulation.Features.Inventory.Application;
using Game.Simulation.Features.Inventory.Contracts;
using Game.Simulation.Features.Parties.Contracts;

namespace BannersAndLords.Simulation.Tests;

public sealed class PartyInventoryTests
{
    private static PartyInventory Create(params InventoryStack[] stacks) => new(
        new PartyIdentity("test-party", "test-commander"), StartingInventory.CreateCatalog(), stacks);

    [Fact]
    public void EmptyInventoryIsBoundToItsPartyAndKnownGoodsHaveZeroQuantity()
    {
        var inventory = Create();
        Assert.Equal("test-party", inventory.PartyId);
        Assert.Empty(inventory.State.Stacks);
        Assert.Equal(0, inventory.GetStackQuantity("grain"));
        Assert.False(inventory.ContainsStack("grain", 1));
        Assert.Throws<KeyNotFoundException>(() => inventory.GetStackQuantity("unknown"));
    }

    [Fact]
    public void AddingMergesStacksAndExactRemovalRemovesTheStack()
    {
        var inventory = Create(new InventoryStack("grain", 20));
        Assert.Equal(InventoryResult.Success, inventory.AddStack("grain", 7));
        Assert.Equal(InventoryResult.Success, inventory.AddStack("tools", 2));
        Assert.Equal(27, inventory.GetStackQuantity("grain"));
        Assert.True(inventory.ContainsStack("grain", 27));
        Assert.False(inventory.ContainsStack("grain", 28));
        Assert.Equal(InventoryResult.Success, inventory.RemoveStack("grain", 7));
        Assert.Equal(20, inventory.GetStackQuantity("grain"));
        Assert.Equal(InventoryResult.Success, inventory.RemoveStack("grain", 20));
        Assert.Equal(0, inventory.GetStackQuantity("grain"));
        Assert.Equal(new InventoryStack("tools", 2), Assert.Single(inventory.State.Stacks));
    }

    [Theory]
    [InlineData("unknown", 1)]
    [InlineData("GRAIN", 1)]
    [InlineData("", 1)]
    [InlineData(" ", 1)]
    [InlineData("grain", 0)]
    [InlineData("grain", -1)]
    [InlineData("grain", int.MinValue)]
    [InlineData("grain", int.MaxValue)]
    public void InvalidAddAndRemoveLeaveAllInventoryUnchanged(string definitionId, int quantity)
    {
        var inventory = Create(new InventoryStack("grain", 20), new InventoryStack("tools", 2));
        var before = inventory.State.Stacks;
        Assert.NotEqual(InventoryResult.Success, inventory.AddStack(definitionId, quantity));
        Assert.NotEqual(InventoryResult.Success, inventory.RemoveStack(definitionId, quantity));
        Assert.False(inventory.ContainsStack(definitionId, quantity));
        Assert.Same(before, inventory.State.Stacks);
        Assert.Equal([new InventoryStack("grain", 20), new InventoryStack("tools", 2)], inventory.State.Stacks);
    }

    [Fact]
    public void RemovingMoreThanAvailableDoesNotPartiallyRemoveGoods()
    {
        var inventory = Create(new InventoryStack("fish", 8));
        Assert.NotEqual(InventoryResult.Success, inventory.RemoveStack("fish", 9));
        Assert.Equal(8, inventory.GetStackQuantity("fish"));
        Assert.NotEqual(InventoryResult.Success, inventory.RemoveStack("grain", 1));
        Assert.Equal(new InventoryStack("fish", 8), Assert.Single(inventory.State.Stacks));
    }

    [Fact]
    public void MaximumQuantityIsSupportedWithoutOverflowOrNegativeInventory()
    {
        var inventory = Create();
        Assert.Equal(InventoryResult.Success, inventory.AddStack("grain", int.MaxValue));
        Assert.NotEqual(InventoryResult.Success, inventory.AddStack("grain", 1));
        Assert.Equal(int.MaxValue, inventory.GetStackQuantity("grain"));
        Assert.Equal(InventoryResult.Success, inventory.RemoveStack("grain", int.MaxValue));
        Assert.Empty(inventory.State.Stacks);
        Assert.Equal(InventoryResult.Success, inventory.AddStack("grain", 1));
        Assert.Equal(1, inventory.GetStackQuantity("grain"));
    }

    [Fact]
    public void SnapshotsAreReadOnlyAndPreviousSnapshotsDoNotChange()
    {
        var inventory = Create(new InventoryStack("grain", 20));
        var before = inventory.State.Stacks;
        var list = Assert.IsAssignableFrom<IList<InventoryStack>>(before);
        Assert.Throws<NotSupportedException>(() => list[0] = new InventoryStack("grain", 99));
        inventory.AddStack("grain", 3);
        Assert.Equal(new InventoryStack("grain", 20), Assert.Single(before));
        Assert.Equal(new InventoryStack("grain", 23), Assert.Single(inventory.State.Stacks));
    }

    [Fact]
    public void SourceInventoryIsCopiedAndDuplicateStartingStacksAreMergedInStableOrder()
    {
        var source = new[] { new InventoryStack("tools", 2), new InventoryStack("grain", 20), new InventoryStack("grain", 3) };
        var inventory = Create(source);
        source[0] = new InventoryStack("fish", 99);
        Assert.Equal([new InventoryStack("grain", 23), new InventoryStack("tools", 2)], inventory.State.Stacks);
    }

    [Fact]
    public void PartiesSharingACatalogHaveIndependentInventories()
    {
        var catalog = StartingInventory.CreateCatalog();
        var first = StartingInventory.CreateFor(new PartyIdentity("first", "aldric"), catalog);
        var second = StartingInventory.CreateFor(new PartyIdentity("second", "rowan"), catalog);
        Assert.Equal(InventoryResult.Success, first.RemoveStack("grain", 20));
        Assert.Equal(InventoryResult.Success, first.AddStack("bread", 4));
        Assert.Equal("first", first.PartyId);
        Assert.Equal("second", second.PartyId);
        Assert.Equal(20, second.GetStackQuantity("grain"));
        Assert.Equal(60, second.GetStackQuantity("bread"));
    }

    [Fact]
    public void InvalidStartingInventoryAndMissingPartyIdAreRejected()
    {
        Assert.Throws<ArgumentException>(() => Create(new InventoryStack("unknown", 1)));
        Assert.Throws<ArgumentException>(() => Create(new InventoryStack("grain", 0)));
        Assert.Throws<ArgumentException>(() => Create(new InventoryStack("grain", -1)));
        Assert.Throws<ArgumentException>(() => Create(new InventoryStack("grain", int.MaxValue), new InventoryStack("grain", 1)));
        Assert.Throws<ArgumentException>(() => new PartyInventory(new PartyIdentity(" ", "aldric"), StartingInventory.CreateCatalog()));
    }

    [Fact]
    public void CatalogSeparatesDefinitionsFromQuantitiesAndCannotBeMutatedByQueries()
    {
        var source = new[] { new ItemDefinition("grain", "Grain", ItemCategory.Food, ItemStorage.Stack, "Grain for provisions.") };
        var catalog = new ItemCatalog(source);
        source[0] = source[0] with { Name = "Changed" };
        Assert.Equal("Grain", catalog.GetDefinition("grain").Name);
        Assert.False(catalog.Contains("unknown"));
        Assert.Throws<KeyNotFoundException>(() => catalog.GetDefinition("unknown"));
        var list = Assert.IsAssignableFrom<IList<ItemDefinition>>(catalog.Definitions);
        Assert.Throws<NotSupportedException>(() => list[0] = source[0]);
    }

    [Fact]
    public void CatalogRejectsDuplicateIdsAndUnsupportedCategories()
    {
        var definition = new ItemDefinition("grain", "Grain", ItemCategory.Food, ItemStorage.Stack, "Grain for provisions.");
        Assert.Throws<ArgumentException>(() => new ItemCatalog([definition, definition]));
        Assert.Throws<ArgumentException>(() => new ItemCatalog([definition with { Category = (ItemCategory)99 }]));
    }

    [Theory]
    [InlineData("", "Grain")]
    [InlineData(" ", "Grain")]
    [InlineData("grain", "")]
    [InlineData("grain", " ")]
    public void CatalogRequiresNonBlankIdsAndNames(string id, string name)
    {
        Assert.Throws<ArgumentException>(() => new ItemCatalog([new ItemDefinition(id, name, ItemCategory.Food, ItemStorage.Stack, "Grain for provisions.")]));
    }
}
