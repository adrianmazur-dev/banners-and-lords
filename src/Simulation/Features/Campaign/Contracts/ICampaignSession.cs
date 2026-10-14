namespace Game.Simulation.Features.Campaign.Contracts;

public interface ICampaignSession
{
    CampaignStatus Status { get; }
    void Advance(double elapsedRealSeconds);
    void SetPaused(bool isPaused);
    void SetSpeed(CampaignSpeed speed);
}
