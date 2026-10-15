namespace Game.Simulation.Features.Settlements.Contracts;

public interface ISettlementVisits
{
    IReadOnlyList<SettlementDefinition> Settlements { get; }
    string? CurrentSettlementId { get; }
    SettlementDefinition GetSettlement(string settlementId);
    bool CanEnter(string settlementId);
    bool TryTravelTo(string settlementId);
    bool TryEnter(string settlementId);
    bool TryLeave();
}
