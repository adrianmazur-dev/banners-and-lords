using System.Numerics;
using Game.Simulation.Features.Campaign.Application;
using Game.Simulation.Features.Campaign.Contracts;
using Game.Simulation.Features.Parties.Application;

namespace BannersAndLords.Simulation.Tests;

public sealed class PartyTravelTests
{
    private static PartyTravel CreateParty() => new(new Vector2(1200, 700), new Vector2(100, 100), 3);

    [Fact]
    public void OrderingTheCurrentPositionCancelsTravelWithoutChangingTheRenderedSegment()
    {
        var party = CreateParty();
        party.TrySetDestination(new Vector2(500, 100));
        party.AdvanceTick(1, 10);
        var before = party.Status;
        Assert.True(party.TrySetDestination(before.Position));
        Assert.Null(party.Status.Destination);
        Assert.Equal(before.Position, party.Status.Position);
        Assert.Equal(before.PreviousPosition, party.Status.PreviousPosition);
    }

    [Fact]
    public void StaysStillWithoutAnOrder()
    {
        var party = CreateParty();
        party.AdvanceTick(1, 10);
        Assert.Equal(new Vector2(100, 100), party.Status.Position);
        Assert.Null(party.Status.Destination);
    }

    [Fact]
    public void MovesInAStraightLineAndStopsExactlyAtTheDestination()
    {
        var party = CreateParty();
        party.TrySetDestination(new Vector2(130, 140));
        party.AdvanceTick(1, 10);
        Assert.Equal(new Vector2(118, 124), party.Status.Position);
        Assert.Equal(new Vector2(100, 100), party.Status.PreviousPosition);

        party.AdvanceTick(2, 10);
        Assert.Equal(new Vector2(130, 140), party.Status.Position);
        Assert.Null(party.Status.Destination);
        party.AdvanceTick(3, 10);
        Assert.Equal(party.Status.Position, party.Status.PreviousPosition);
    }

    [Theory]
    [InlineData(-1, 100)]
    [InlineData(1201, 100)]
    [InlineData(100, 701)]
    [InlineData(float.NaN, 100)]
    [InlineData(100, float.PositiveInfinity)]
    public void InvalidDestinationPreservesTheExistingOrder(float x, float y)
    {
        var party = CreateParty();
        party.TrySetDestination(new Vector2(400, 100));
        var before = party.Status;
        Assert.False(party.TrySetDestination(new Vector2(x, y)));
        Assert.Equal(before, party.Status);
    }

    [Fact]
    public void RedirectingKeepsThePreviousSegmentForContinuousPresentation()
    {
        var party = CreateParty();
        party.TrySetDestination(new Vector2(400, 100));
        party.AdvanceTick(1, 10);
        var before = party.Status;
        party.TrySetDestination(new Vector2(130, 400));
        Assert.Equal(before.Position, party.Status.Position);
        Assert.Equal(before.PreviousPosition, party.Status.PreviousPosition);
        party.AdvanceTick(2, 10);
        Assert.Equal(new Vector2(130, 130), party.Status.Position);
    }

    [Fact]
    public void AnOrderCanBePlacedDuringPauseWithoutMovingOrResumingTime()
    {
        var party = CreateParty();
        var session = new CampaignSession(systems: [party]);
        party.TrySetDestination(new Vector2(900, 100));
        session.Advance(0.15);
        session.SetPaused(true);
        var before = party.Status;
        party.TrySetDestination(new Vector2(103, 500));
        session.Advance(100);
        Assert.Equal(before.Position, party.Status.Position);
        Assert.Equal(before.PreviousPosition, party.Status.PreviousPosition);
        Assert.Equal(0.5, session.TickProgress, 8);
        Assert.True(session.Status.IsPaused);
        session.SetPaused(false);
        session.Advance(0.05);
        Assert.Equal(new Vector2(103, 103), party.Status.Position);
    }

    [Fact]
    public void NewOrdersAndRedirectionTakeEffectWithinOneTenthOfASecond()
    {
        var party = CreateParty();
        var session = new CampaignSession(systems: [party]);
        party.TrySetDestination(new Vector2(900, 100));
        session.Advance(0.1);
        Assert.Equal(new Vector2(103, 100), party.Status.Position);

        party.TrySetDestination(new Vector2(103, 500));
        var before = party.Status;
        session.Advance(0.099);
        Assert.Equal(before, party.Status);
        session.Advance(0.001);
        Assert.Equal(before.Position, party.Status.PreviousPosition);
        Assert.Equal(new Vector2(103, 103), party.Status.Position);
    }

    [Fact]
    public void DefaultTravelSpeedStillCoversTwentyFourUnitsPerRealSecond()
    {
        var party = new PartyTravel(new Vector2(1200, 700), new Vector2(100, 100));
        var session = new CampaignSession(systems: [party]);
        party.TrySetDestination(new Vector2(1100, 100));
        session.Advance(1);
        Assert.Equal(10, session.Status.Minute);
        Assert.InRange(party.Status.Position.X, 123.9999f, 124.0001f);
    }

    [Theory]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(144)]
    public void MovementAndInterpolationMatchAcrossFrameRates(int fps)
    {
        var party = CreateParty();
        var session = new CampaignSession(systems: [party]);
        party.TrySetDestination(new Vector2(1100, 100));
        for (var frame = 0; frame < fps * 10 + fps / 2; frame++)
        {
            session.Advance(1.0 / fps);
        }
        session.Advance(0.05);

        Assert.Equal(new Vector2(415, 100), party.Status.Position);
        Assert.Equal(new Vector2(412, 100), party.Status.PreviousPosition);
        Assert.InRange(session.TickProgress, 0.499999, 0.500001);
    }

    [Fact]
    public void CampaignSpeedControlsTravelSpeed()
    {
        var normal = CreateParty();
        var fast = CreateParty();
        var normalSession = new CampaignSession(systems: [normal]);
        var fastSession = new CampaignSession(systems: [fast]);
        normal.TrySetDestination(new Vector2(1100, 100));
        fast.TrySetDestination(new Vector2(1100, 100));
        fastSession.SetSpeed(CampaignSpeed.Fast);
        normalSession.Advance(1);
        fastSession.Advance(1);
        Assert.Equal(30, normal.Status.Position.X - 100);
        Assert.Equal(60, fast.Status.Position.X - 100);
    }
}
