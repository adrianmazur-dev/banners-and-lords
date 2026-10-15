namespace Game.Simulation.Features.Inventory.Contracts;

public interface IItemCatalog
{
    IReadOnlyList<ItemDefinition> Definitions { get; }
    bool Contains(string definitionId);
    ItemDefinition GetDefinition(string definitionId);
}
