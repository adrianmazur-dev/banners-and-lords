namespace Game.Simulation.Features.Campaign.Contracts;

public interface ICampaignTickSystem
{
    void AdvanceTick(long tick, int elapsedGameMinutes);
}
