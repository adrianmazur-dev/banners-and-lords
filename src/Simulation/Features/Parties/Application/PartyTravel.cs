using System.Numerics;
using Game.Simulation.Features.Campaign.Contracts;
using Game.Simulation.Features.Parties.Contracts;
using Game.Simulation.Features.Parties.Domain;

namespace Game.Simulation.Features.Parties.Application;

public sealed class PartyTravel : IPartyTravel, ICampaignTickSystem
{
    private readonly PartyMovement _movement;

    public PartyTravel(Vector2 mapSize, Vector2 start, float unitsPerGameMinute = 2.4f)
    {
        _movement = new PartyMovement(mapSize, start, unitsPerGameMinute);
    }

    public PartyStatus Status => new(
        _movement.PreviousPosition, _movement.Position, _movement.Destination);

    public bool TrySetDestination(Vector2 destination) => _movement.TrySetDestination(destination);

    public void AdvanceTick(long tick, int elapsedGameMinutes) => _movement.Advance(elapsedGameMinutes);
}
