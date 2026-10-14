using Game.Simulation.Features.Campaign.Contracts;
using Game.Simulation.Features.Campaign.Domain;

namespace Game.Simulation.Features.Campaign.Application;

public sealed class CampaignSession : ICampaignSession
{
    public const int MaxTicksPerAdvance = 120;
    private const double TickTolerance = 1e-9;

    private readonly CampaignClock _clock = new();
    private readonly double _secondsPerTick;
    private double _pendingTicks;
    private bool _isPaused;
    private CampaignSpeed _speed = CampaignSpeed.Normal;

    public CampaignSession(double secondsPerTick = 1.0)
    {
        if (!double.IsFinite(secondsPerTick) || secondsPerTick <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(secondsPerTick));
        }

        _secondsPerTick = secondsPerTick;
    }

    public CampaignStatus Status => new(
        _clock.Tick, _clock.Day, _clock.Hour, _clock.Minute, _isPaused, _speed);

    public void Advance(double elapsedRealSeconds)
    {
        if (!double.IsFinite(elapsedRealSeconds) || elapsedRealSeconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(elapsedRealSeconds));
        }

        if (_isPaused)
        {
            return;
        }

        var pendingTicks = _pendingTicks + elapsedRealSeconds * (int)_speed / _secondsPerTick;
        if (!double.IsFinite(pendingTicks))
        {
            throw new ArgumentOutOfRangeException(nameof(elapsedRealSeconds));
        }

        _pendingTicks = pendingTicks;
        for (var count = 0; count < MaxTicksPerAdvance && _pendingTicks + TickTolerance >= 1; count++)
        {
            _clock.AdvanceTick();
            // Absorb floating-point error at tick boundaries, including common frame intervals.
            _pendingTicks = Math.Max(0, _pendingTicks - 1);
        }
    }

    public void SetPaused(bool isPaused) => _isPaused = isPaused;

    public void SetSpeed(CampaignSpeed speed)
    {
        if (speed is not (CampaignSpeed.Normal or CampaignSpeed.Fast))
        {
            throw new ArgumentOutOfRangeException(nameof(speed));
        }

        _speed = speed;
    }
}
