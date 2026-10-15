using System.Numerics;

namespace Game.Simulation.Features.Parties.Domain;

internal sealed class PartyMovement
{
    private readonly Vector2 _mapSize;
    private readonly float _unitsPerGameMinute;

    public Vector2 PreviousPosition { get; private set; }
    public Vector2 Position { get; private set; }
    public Vector2? Destination { get; private set; }

    public PartyMovement(Vector2 mapSize, Vector2 start, float unitsPerGameMinute)
    {
        if (!IsFinite(mapSize) || mapSize.X <= 0 || mapSize.Y <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(mapSize));
        }

        _mapSize = mapSize;
        if (!Contains(start))
        {
            throw new ArgumentOutOfRangeException(nameof(start));
        }

        if (!float.IsFinite(unitsPerGameMinute) || unitsPerGameMinute <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitsPerGameMinute));
        }

        PreviousPosition = Position = start;
        _unitsPerGameMinute = unitsPerGameMinute;
    }

    public bool TrySetDestination(Vector2 destination)
    {
        if (!Contains(destination))
        {
            return false;
        }

        Destination = destination == Position ? null : destination;
        return true;
    }

    public void Stop()
    {
        Destination = null;
        PreviousPosition = Position;
    }

    public void Advance(int elapsedGameMinutes)
    {
        if (elapsedGameMinutes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(elapsedGameMinutes));
        }

        PreviousPosition = Position;
        if (Destination is not { } destination)
        {
            return;
        }

        var offset = destination - Position;
        var distance = offset.Length();
        var travel = _unitsPerGameMinute * elapsedGameMinutes;
        if (travel >= distance)
        {
            Position = destination;
            Destination = null;
        }
        else
        {
            Position += offset / distance * travel;
        }
    }

    private bool Contains(Vector2 point) => IsFinite(point)
        && point.X >= 0 && point.X <= _mapSize.X
        && point.Y >= 0 && point.Y <= _mapSize.Y;

    private static bool IsFinite(Vector2 point) => float.IsFinite(point.X) && float.IsFinite(point.Y);
}
