namespace Game.Simulation.Features.Campaign.Contracts;

public interface ICampaignSession
{
    CampaignStatus Status { get; }
    double TickProgress { get; }
    bool IsPauseLocked { get; }
    IDisposable HoldPause();
    void Advance(double elapsedRealSeconds);
    void SetPaused(bool isPaused);
    void SetSpeed(CampaignSpeed speed);
}
