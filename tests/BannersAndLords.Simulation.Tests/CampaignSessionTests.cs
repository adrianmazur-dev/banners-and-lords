using Game.Simulation.Features.Campaign.Application;
using Game.Simulation.Features.Campaign.Contracts;

namespace BannersAndLords.Simulation.Tests;

public sealed class CampaignSessionTests
{
    [Fact]
    public void StartsAtMidnightOnDayOneAtNormalSpeed()
    {
        Assert.Equal(new CampaignStatus(0, 1, 0, 0, false, CampaignSpeed.Normal), new CampaignSession().Status);
    }

    [Fact]
    public void AdvancesOnlyCompleteTicksAndPreservesFractionalTime()
    {
        var session = new CampaignSession();
        session.Advance(0.75);
        Assert.Equal(0, session.Status.Tick);

        session.Advance(1.5);
        Assert.Equal(2, session.Status.Tick);
        Assert.Equal(20, session.Status.Minute);

        session.Advance(0.75);
        Assert.Equal(3, session.Status.Tick);
    }

    [Theory]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(144)]
    public void EqualElapsedTimeProducesTheSameClockAcrossFrameRates(int framesPerSecond)
    {
        var session = new CampaignSession();
        var batched = new CampaignSession();
        for (var frame = 0; frame < framesPerSecond * 60; frame++)
        {
            session.Advance(1.0 / framesPerSecond);
        }

        batched.Advance(60);
        Assert.Equal(batched.Status, session.Status);
        Assert.Equal(60, session.Status.Tick);
    }

    [Fact]
    public void RollsOverToTheNextDay()
    {
        var session = new CampaignSession();
        session.Advance(120);
        session.Advance(23);
        Assert.Equal(new CampaignStatus(143, 1, 23, 50, false, CampaignSpeed.Normal), session.Status);

        session.Advance(1);
        Assert.Equal(new CampaignStatus(144, 2, 0, 0, false, CampaignSpeed.Normal), session.Status);
    }

    [Fact]
    public void PausePreservesFractionalTimeWithoutAccumulatingPausedTime()
    {
        var session = new CampaignSession();
        session.Advance(0.75);
        session.SetPaused(true);
        var paused = session.Status;
        session.Advance(100);
        Assert.Equal(paused, session.Status);

        session.SetPaused(false);
        session.Advance(0.25);
        Assert.Equal(1, session.Status.Tick);
    }

    [Fact]
    public void FastSpeedDoublesTheNumberOfTicks()
    {
        var normal = new CampaignSession();
        var fast = new CampaignSession();
        fast.SetSpeed(CampaignSpeed.Fast);
        normal.Advance(10);
        fast.Advance(10);
        Assert.Equal(normal.Status.Tick * 2, fast.Status.Tick);
    }

    [Fact]
    public void ChangingSpeedDuringPauseDoesNotResumeTheCampaign()
    {
        var session = new CampaignSession();
        session.SetPaused(true);
        session.SetSpeed(CampaignSpeed.Fast);
        session.Advance(10);
        Assert.True(session.Status.IsPaused);
        Assert.Equal(CampaignSpeed.Fast, session.Status.Speed);
        Assert.Equal(0, session.Status.Tick);

        session.SetPaused(false);
        session.Advance(1);
        Assert.Equal(2, session.Status.Tick);
    }

    [Fact]
    public void SpeedChangesPreserveProgressTowardTheNextTick()
    {
        var session = new CampaignSession();
        session.Advance(0.5);
        session.SetSpeed(CampaignSpeed.Fast);
        session.Advance(0.25);
        Assert.Equal(1, session.Status.Tick);

        session.SetSpeed(CampaignSpeed.Normal);
        session.Advance(1);
        Assert.Equal(2, session.Status.Tick);
    }

    [Fact]
    public void LongFramesKeepBacklogWithoutProcessingUnboundedWork()
    {
        var session = new CampaignSession();
        session.Advance(300.5);
        Assert.Equal(CampaignSession.MaxTicksPerAdvance, session.Status.Tick);
        session.Advance(0);
        session.Advance(0);
        Assert.Equal(300, session.Status.Tick);
        session.Advance(0.5);
        Assert.Equal(301, session.Status.Tick);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void InvalidElapsedTimeDoesNotChangeState(double elapsed)
    {
        var session = new CampaignSession();
        var before = session.Status;
        Assert.Throws<ArgumentOutOfRangeException>(() => session.Advance(elapsed));
        Assert.Equal(before, session.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void InvalidTickDurationIsRejected(double secondsPerTick)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CampaignSession(secondsPerTick));
    }

    [Fact]
    public void UnsupportedSpeedIsRejectedWithoutChangingState()
    {
        var session = new CampaignSession();
        Assert.Throws<ArgumentOutOfRangeException>(() => session.SetSpeed((CampaignSpeed)3));
        Assert.Equal(CampaignSpeed.Normal, session.Status.Speed);
    }
}
