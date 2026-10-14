namespace Game.Simulation.Features.Campaign.Domain;

internal sealed class CampaignClock
{
    public const int MinutesPerTick = 1;
    private const int TicksPerDay = 24 * 60 / MinutesPerTick;

    public long Tick { get; private set; }
    public long Day => Tick / TicksPerDay + 1;
    public int Hour => (int)(Tick % TicksPerDay) * MinutesPerTick / 60;
    public int Minute => (int)(Tick % TicksPerDay) * MinutesPerTick % 60;

    public void AdvanceTick()
    {
        Tick = checked(Tick + 1);
    }
}
