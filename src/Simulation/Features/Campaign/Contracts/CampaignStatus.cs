namespace Game.Simulation.Features.Campaign.Contracts;

public readonly record struct CampaignStatus(
    long Tick,
    long Day,
    int Hour,
    int Minute,
    bool IsPaused,
    CampaignSpeed Speed);
