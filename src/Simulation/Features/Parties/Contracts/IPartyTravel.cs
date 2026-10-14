using System.Numerics;

namespace Game.Simulation.Features.Parties.Contracts;

public interface IPartyTravel
{
    PartyIdentity Identity { get; }
    PartyStatus Status { get; }
    bool TrySetDestination(Vector2 destination);
}
