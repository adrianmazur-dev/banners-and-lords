using System.Numerics;
using Game.Simulation.Features.Campaign.Contracts;
using Game.Simulation.Features.Dynasties.Contracts;
using Game.Simulation.Features.Parties.Contracts;
using Game.Simulation.Features.Parties.Domain;

namespace Game.Simulation.Features.Parties.Application;

public sealed class PartyTravel : IPartyTravel, ICampaignTickSystem
{
    private readonly PartyMovement _movement;

    public PartyTravel(PartyIdentity identity, IDynastyQuery dynasties,
        Vector2 mapSize, Vector2 start, float unitsPerGameMinute = 2.4f)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identity.Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(identity.CommanderId);
        dynasties.GetCharacter(identity.CommanderId);
        Identity = identity;
        _movement = new PartyMovement(mapSize, start, unitsPerGameMinute);
    }

    public PartyIdentity Identity { get; }

    public PartyStatus Status => new(
        _movement.PreviousPosition, _movement.Position, _movement.Destination);

    public bool TrySetDestination(Vector2 destination) => _movement.TrySetDestination(destination);
    public void Stop() => _movement.Stop();

    public void AdvanceTick(long tick, int elapsedGameMinutes) => _movement.Advance(elapsedGameMinutes);
}
