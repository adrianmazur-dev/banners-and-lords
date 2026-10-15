using Game.Simulation.Features.Inventory.Application;
using Game.Simulation.Features.Inventory.Contracts;
using Game.Simulation.Features.Parties.Contracts;

namespace BannersAndLords.Simulation.Tests;

public sealed class PartyInventoryTests
{
    private static PartyInventory Create(params InventoryStack[] cargo) => new(
        new PartyIdentity("test-party", "test-commander"), StartingInventory.CreateCatalog(), cargo);

    [Fact]
    public void EmptyInventoryIsBoundToItsPartyAndKnownGoodsHaveZeroQuantity()
    {
        var inventory = Create();
        Assert.Equal("test-party", inventory.PartyId);
        Assert.Empty(inventory.Stacks);
        Assert.Equal(0, inventory.GetQuantity("grain"));
        Assert.False(inventory.Contains("grain", 1));
        Assert.Throws<KeyNotFoundException>(() => inventory.GetQuantity("unknown"));
    }

    [Fact]
    public void AddingMergesStacksAndExactRemovalRemovesTheStack()
    {
        var inventory = Create(new InventoryStack("grain", 20));
        Assert.True(inventory.TryAdd("grain", 7));
        Assert.True(inventory.TryAdd("tools", 2));
        Assert.Equal(27, inventory.GetQuantity("grain"));
        Assert.True(inventory.Contains("grain", 27));
        Assert.False(inventory.Contains("grain", 28));
        Assert.True(inventory.TryRemove("grain", 7));
        Assert.Equal(20, inventory.GetQuantity("grain"));
        Assert.True(inventory.TryRemove("grain", 20));
        Assert.Equal(0, inventory.GetQuantity("grain"));
        Assert.Equal(new InventoryStack("tools", 2), Assert.Single(inventory.Stacks));
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
    public void InvalidAddAndRemoveLeaveAllCargoUnchanged(string goodsId, int quantity)
    {
        var inventory = Create(new InventoryStack("grain", 20), new InventoryStack("tools", 2));
        var before = inventory.Stacks;
        Assert.False(inventory.TryAdd(goodsId, quantity));
        Assert.False(inventory.TryRemove(goodsId, quantity));
        Assert.False(inventory.Contains(goodsId, quantity));
        Assert.Same(before, inventory.Stacks);
        Assert.Equal([new InventoryStack("grain", 20), new InventoryStack("tools", 2)], inventory.Stacks);
    }

    [Fact]
    public void RemovingMoreThanAvailableDoesNotPartiallyRemoveGoods()
    {
        var inventory = Create(new InventoryStack("fish", 8));
        Assert.False(inventory.TryRemove("fish", 9));
        Assert.Equal(8, inventory.GetQuantity("fish"));
        Assert.False(inventory.TryRemove("grain", 1));
        Assert.Equal(new InventoryStack("fish", 8), Assert.Single(inventory.Stacks));
    }

    [Fact]
    public void MaximumQuantityIsSupportedWithoutOverflowOrNegativeCargo()
    {
        var inventory = Create();
        Assert.True(inventory.TryAdd("grain", int.MaxValue));
        Assert.False(inventory.TryAdd("grain", 1));
        Assert.Equal(int.MaxValue, inventory.GetQuantity("grain"));
        Assert.True(inventory.TryRemove("grain", int.MaxValue));
        Assert.Empty(inventory.Stacks);
        Assert.True(inventory.TryAdd("grain", 1));
        Assert.Equal(1, inventory.GetQuantity("grain"));
    }

    [Fact]
    public void SnapshotsAreReadOnlyAndPreviousSnapshotsDoNotChange()
    {
        var inventory = Create(new InventoryStack("grain", 20));
        var before = inventory.Stacks;
        var list = Assert.IsAssignableFrom<IList<InventoryStack>>(before);
        Assert.Throws<NotSupportedException>(() => list[0] = new InventoryStack("grain", 99));
        inventory.TryAdd("grain", 3);
        Assert.Equal(new InventoryStack("grain", 20), Assert.Single(before));
        Assert.Equal(new InventoryStack("grain", 23), Assert.Single(inventory.Stacks));
    }

    [Fact]
    public void SourceCargoIsCopiedAndDuplicateStartingStacksAreMergedInStableOrder()
    {
        var source = new[] { new InventoryStack("tools", 2), new InventoryStack("grain", 20), new InventoryStack("grain", 3) };
        var inventory = Create(source);
        source[0] = new InventoryStack("fish", 99);
        Assert.Equal([new InventoryStack("grain", 23), new InventoryStack("tools", 2)], inventory.Stacks);
    }

    [Fact]
    public void PartiesSharingACatalogHaveIndependentInventories()
    {
        var catalog = StartingInventory.CreateCatalog();
        var first = StartingInventory.CreateFor(new PartyIdentity("first", "aldric"), catalog);
        var second = StartingInventory.CreateFor(new PartyIdentity("second", "rowan"), catalog);
        Assert.True(first.TryRemove("grain", 20));
        Assert.True(first.TryAdd("bread", 4));
        Assert.Equal("first", first.PartyId);
        Assert.Equal("second", second.PartyId);
        Assert.Equal(20, second.GetQuantity("grain"));
        Assert.Equal(0, second.GetQuantity("bread"));
    }

    [Fact]
    public void InvalidStartingCargoAndMissingPartyIdAreRejected()
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
        var source = new[] { new GoodsDefinition("grain", "Grain", GoodsCategory.Food) };
        var catalog = new GoodsCatalog(source);
        source[0] = source[0] with { Name = "Changed" };
        Assert.Equal("Grain", catalog.GetDefinition("grain").Name);
        Assert.False(catalog.Contains("unknown"));
        Assert.Throws<KeyNotFoundException>(() => catalog.GetDefinition("unknown"));
        var list = Assert.IsAssignableFrom<IList<GoodsDefinition>>(catalog.Definitions);
        Assert.Throws<NotSupportedException>(() => list[0] = source[0]);
    }

    [Fact]
    public void CatalogRejectsDuplicateIdsAndUnsupportedCategories()
    {
        var definition = new GoodsDefinition("grain", "Grain", GoodsCategory.Food);
        Assert.Throws<ArgumentException>(() => new GoodsCatalog([definition, definition]));
        Assert.Throws<ArgumentException>(() => new GoodsCatalog([definition with { Category = (GoodsCategory)99 }]));
    }

    [Theory]
    [InlineData("", "Grain")]
    [InlineData(" ", "Grain")]
    [InlineData("grain", "")]
    [InlineData("grain", " ")]
    public void CatalogRequiresNonBlankIdsAndNames(string id, string name)
    {
        Assert.Throws<ArgumentException>(() => new GoodsCatalog([new GoodsDefinition(id, name, GoodsCategory.Food)]));
    }
}
