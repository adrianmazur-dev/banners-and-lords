using System.Numerics;
using Game.Simulation.Features.Campaign.Contracts;
using Game.Simulation.Features.Parties.Contracts;
using Game.Simulation.Features.Settlements.Contracts;

namespace Game.Simulation.Features.Settlements.Application;

public sealed class SettlementVisits : ISettlementVisits
{
    private readonly Dictionary<string, SettlementDefinition> _settlements = new(StringComparer.Ordinal);
    private readonly IPartyTravel _party;
    private readonly ICampaignSession _session;
    private IDisposable? _pauseHold;

    public SettlementVisits(IEnumerable<SettlementDefinition> settlements, Vector2 mapSize,
        IPartyTravel party, ICampaignSession session)
    {
        if (!float.IsFinite(mapSize.X) || !float.IsFinite(mapSize.Y) || mapSize.X <= 0 || mapSize.Y <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(mapSize));
        }
        foreach (var settlement in settlements)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(settlement.Id);
            ArgumentException.ThrowIfNullOrWhiteSpace(settlement.Name);
            ArgumentException.ThrowIfNullOrWhiteSpace(settlement.Description);
            var position = settlement.Position;
            if (!float.IsFinite(position.X) || !float.IsFinite(position.Y)
                || position.X < 0 || position.Y < 0 || position.X > mapSize.X || position.Y > mapSize.Y
                || !float.IsFinite(settlement.EntryRadius) || settlement.EntryRadius <= 0)
            {
                throw new ArgumentException($"Settlement '{settlement.Id}' has invalid location or entry radius.", nameof(settlements));
            }
            if (!_settlements.TryAdd(settlement.Id, settlement))
            {
                throw new ArgumentException($"Duplicate settlement ID '{settlement.Id}'.", nameof(settlements));
            }
        }
        Settlements = Array.AsReadOnly(_settlements.Values.ToArray());
        _party = party;
        _session = session;
    }

    public IReadOnlyList<SettlementDefinition> Settlements { get; }
    public string? CurrentSettlementId { get; private set; }
    public SettlementDefinition GetSettlement(string settlementId) => _settlements[settlementId];

    public bool CanEnter(string settlementId) => CurrentSettlementId is null
        && _settlements.TryGetValue(settlementId, out var settlement)
        && Vector2.Distance(_party.Status.Position, settlement.Position) <= settlement.EntryRadius;

    public bool TryTravelTo(string settlementId) => CurrentSettlementId is null
        && _settlements.TryGetValue(settlementId, out var settlement)
        && _party.TrySetDestination(settlement.Position);

    public bool TryEnter(string settlementId)
    {
        if (!CanEnter(settlementId))
        {
            return false;
        }
        _pauseHold = _session.HoldPause();
        _party.Stop();
        CurrentSettlementId = settlementId;
        return true;
    }

    public bool TryLeave()
    {
        if (CurrentSettlementId is null)
        {
            return false;
        }
        CurrentSettlementId = null;
        _pauseHold!.Dispose();
        _pauseHold = null;
        return true;
    }
}
