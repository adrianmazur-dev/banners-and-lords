using Game.Simulation.Features.Inventory.Contracts;

namespace Game.Simulation.Features.Inventory.Application;

public sealed class GoodsCatalog : IGoodsCatalog
{
    private readonly Dictionary<string, GoodsDefinition> _definitions = new(StringComparer.Ordinal);

    public GoodsCatalog(IEnumerable<GoodsDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        foreach (var definition in definitions)
        {
            ArgumentNullException.ThrowIfNull(definition);
            ArgumentException.ThrowIfNullOrWhiteSpace(definition.Id);
            ArgumentException.ThrowIfNullOrWhiteSpace(definition.Name);
            if (!Enum.IsDefined(definition.Category))
            {
                throw new ArgumentException($"Unknown category for goods '{definition.Id}'.", nameof(definitions));
            }
            if (!_definitions.TryAdd(definition.Id, definition))
            {
                throw new ArgumentException($"Duplicate goods ID '{definition.Id}'.", nameof(definitions));
            }
        }
        Definitions = Array.AsReadOnly(_definitions.Values.OrderBy(definition => definition.Id, StringComparer.Ordinal).ToArray());
    }

    public IReadOnlyList<GoodsDefinition> Definitions { get; }
    public bool Contains(string goodsId) => !string.IsNullOrWhiteSpace(goodsId) && _definitions.ContainsKey(goodsId);
    public GoodsDefinition GetDefinition(string goodsId) => _definitions[goodsId];
}
