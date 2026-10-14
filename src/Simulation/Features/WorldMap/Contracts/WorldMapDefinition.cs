using System.Numerics;

namespace Game.Simulation.Features.WorldMap.Contracts;

public sealed record WorldMapDefinition(
    Vector2 Size,
    IReadOnlyList<MapLandmark> Landmarks,
    IReadOnlyList<Vector2> Road,
    IReadOnlyList<Vector2> River);
