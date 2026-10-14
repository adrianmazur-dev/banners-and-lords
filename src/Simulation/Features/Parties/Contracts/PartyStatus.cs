using System.Numerics;

namespace Game.Simulation.Features.Parties.Contracts;

public readonly record struct PartyStatus(
    Vector2 PreviousPosition,
    Vector2 Position,
    Vector2? Destination);
