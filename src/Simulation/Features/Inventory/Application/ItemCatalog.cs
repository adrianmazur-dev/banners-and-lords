using Game.Simulation.Features.Inventory.Contracts;

namespace Game.Simulation.Features.Inventory.Application;

public sealed class ItemCatalog : IItemCatalog
{
    private readonly Dictionary<string, ItemDefinition> _definitions = new(StringComparer.Ordinal);

    public ItemCatalog(IEnumerable<ItemDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        foreach (var definition in definitions)
        {
            ArgumentNullException.ThrowIfNull(definition);
            ArgumentException.ThrowIfNullOrWhiteSpace(definition.Id);
            ArgumentException.ThrowIfNullOrWhiteSpace(definition.Name);
            ArgumentException.ThrowIfNullOrWhiteSpace(definition.Description);
            var equipment = definition.Category is ItemCategory.Weapons or ItemCategory.Armor;
            if (!Enum.IsDefined(definition.Category) || !Enum.IsDefined(definition.Storage)
                || (definition.Slot.HasValue && !Enum.IsDefined(definition.Slot.Value))
                || (equipment && (definition.Storage != ItemStorage.Individual || definition.Slot is null))
                || (definition.Slot.HasValue && definition.Storage != ItemStorage.Individual))
                throw new ArgumentException($"Invalid item definition '{definition.Id}'.", nameof(definitions));
            if (!_definitions.TryAdd(definition.Id, definition))
                throw new ArgumentException($"Duplicate item ID '{definition.Id}'.", nameof(definitions));
        }
        Definitions = Array.AsReadOnly(_definitions.Values.OrderBy(item => item.Id, StringComparer.Ordinal).ToArray());
    }

    public IReadOnlyList<ItemDefinition> Definitions { get; }
    public bool Contains(string definitionId) => !string.IsNullOrWhiteSpace(definitionId) && _definitions.ContainsKey(definitionId);
    public ItemDefinition GetDefinition(string definitionId) => _definitions[definitionId];
}
