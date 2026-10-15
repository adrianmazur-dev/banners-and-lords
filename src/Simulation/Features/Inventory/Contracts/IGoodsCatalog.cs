namespace Game.Simulation.Features.Inventory.Contracts;

public interface IGoodsCatalog
{
    IReadOnlyList<GoodsDefinition> Definitions { get; }
    bool Contains(string goodsId);
    GoodsDefinition GetDefinition(string goodsId);
}
