using Game.Simulation.Features.Inventory.Contracts;
using Game.Simulation.Features.Parties.Contracts;

namespace Game.Simulation.Features.Inventory.Application;

public static class StartingInventory
{
    public static IGoodsCatalog CreateCatalog() => new GoodsCatalog(
    [
        new("grain", "Grain", GoodsCategory.Food),
        new("bread", "Bread", GoodsCategory.Food),
        new("fish", "Fish", GoodsCategory.Food),
        new("timber", "Timber", GoodsCategory.RawMaterial),
        new("tools", "Tools", GoodsCategory.Manufactured)
    ]);

    public static IPartyInventory CreateFor(PartyIdentity party, IGoodsCatalog catalog) => new PartyInventory(party, catalog,
    [
        new("grain", 20),
        new("fish", 8),
        new("timber", 5),
        new("tools", 2)
    ]);
}
