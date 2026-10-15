using System.Numerics;
using Game.Simulation.Features.Settlements.Contracts;

namespace Game.Simulation.Features.WorldMap.Contracts;

public sealed record WorldMapDefinition(
    Vector2 Size,
    IReadOnlyList<SettlementDefinition> Settlements,
    IReadOnlyList<Vector2> Road,
    IReadOnlyList<Vector2> River);
