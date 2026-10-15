using System.Numerics;

namespace Game.Simulation.Features.Settlements.Contracts;

public sealed record SettlementDefinition(
    string Id, string Name, string Description, Vector2 Position, float EntryRadius = 32);
