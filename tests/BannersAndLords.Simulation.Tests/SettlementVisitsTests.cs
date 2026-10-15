using System.Numerics;
using Game.Simulation.Features.Campaign.Application;
using Game.Simulation.Features.Campaign.Contracts;
using Game.Simulation.Features.Dynasties.Application;
using Game.Simulation.Features.Parties.Application;
using Game.Simulation.Features.Parties.Contracts;
using Game.Simulation.Features.Settlements.Application;
using Game.Simulation.Features.Settlements.Contracts;
using Game.Simulation.Features.WorldMap.Application;

namespace BannersAndLords.Simulation.Tests;

public sealed class SettlementVisitsTests
{
    private static (PartyTravel Party, CampaignSession Session, SettlementVisits Visits) Create(Vector2 start)
    {
        var map = ValleyScenario.CreateMap();
        var party = new PartyTravel(new PartyIdentity("player-party", StartingDynasty.PlayerCharacterId),
            StartingDynasty.CreateRoster(), map.Size, start);
        var session = new CampaignSession(systems: [party]);
        return (party, session, new SettlementVisits(map.Settlements, map.Size, party, session));
    }

    [Fact]
    public void ScenarioProvidesTwoIdentifiableVillagesFromOneSource()
    {
        var (_, _, visits) = Create(ValleyScenario.PlayerStart);
        Assert.Equal(["fallowfield", "stoneford"], visits.Settlements.Select(settlement => settlement.Id));
        Assert.Equal(new Vector2(250, 465), visits.GetSettlement("fallowfield").Position);
        Assert.All(visits.Settlements, settlement => Assert.False(string.IsNullOrWhiteSpace(settlement.Description)));
    }

    [Fact]
    public void DistantEntryFailsWithoutChangingTimeOrTheExistingOrder()
    {
        var (party, session, visits) = Create(ValleyScenario.PlayerStart);
        party.TrySetDestination(new Vector2(900, 585));
        var beforeParty = party.Status;
        var beforeTime = session.Status;
        Assert.False(visits.CanEnter("fallowfield"));
        Assert.False(visits.TryEnter("fallowfield"));
        Assert.Equal(beforeParty, party.Status);
        Assert.Equal(beforeTime, session.Status);
        Assert.Null(visits.CurrentSettlementId);
        Assert.False(session.IsPauseLocked);
    }

    [Theory]
    [InlineData(32, true)]
    [InlineData(32.01f, false)]
    public void EntryUsesTheInclusiveWorldSpaceRadius(float distance, bool expected)
    {
        var (_, _, visits) = Create(new Vector2(250 + distance, 465));
        Assert.Equal(expected, visits.CanEnter("fallowfield"));
        Assert.Equal(expected, visits.TryEnter("fallowfield"));
    }

    [Fact]
    public void DiagonalEntryUsesDistanceRatherThanASquare()
    {
        var (_, _, visits) = Create(new Vector2(274, 489));
        Assert.False(visits.CanEnter("fallowfield"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void VisitHoldsTimeAndRestoresManualPauseSpeedAndFractionalProgress(bool wasPaused)
    {
        var (party, session, visits) = Create(new Vector2(250, 465));
        session.Advance(0.05);
        session.SetSpeed(CampaignSpeed.Fast);
        session.SetPaused(wasPaused);
        party.TrySetDestination(new Vector2(900, 465));
        var before = session.Status;
        Assert.True(visits.TryEnter("fallowfield"));
        Assert.Equal("fallowfield", visits.CurrentSettlementId);
        Assert.Null(party.Status.Destination);
        Assert.True(session.Status.IsPaused);
        Assert.True(session.IsPauseLocked);
        session.SetPaused(false);
        session.SetPaused(true);
        session.Advance(100);
        Assert.Equal(before.Tick, session.Status.Tick);
        Assert.Equal(0.5, session.TickProgress, 8);
        Assert.False(visits.TryEnter("fallowfield"));
        Assert.False(visits.TryEnter("stoneford"));
        Assert.False(visits.TryTravelTo("stoneford"));
        Assert.True(visits.TryLeave());
        Assert.False(visits.TryLeave());
        Assert.Equal(before, session.Status);
        Assert.False(session.IsPauseLocked);
        Assert.Null(visits.CurrentSettlementId);
        session.SetPaused(false);
        session.Advance(0.025);
        Assert.Equal(1, session.Status.Tick);
        Assert.Equal(new Vector2(250, 465), party.Status.Position);
    }

    [Fact]
    public void TravelHerePreservesPauseAndArrivesAtTheSelectedVillage()
    {
        var (party, session, visits) = Create(ValleyScenario.PlayerStart);
        session.SetPaused(true);
        Assert.True(visits.TryTravelTo("fallowfield"));
        Assert.Equal(visits.GetSettlement("fallowfield").Position, party.Status.Destination);
        session.Advance(50);
        Assert.Equal(ValleyScenario.PlayerStart, party.Status.Position);
        session.SetPaused(false);
        session.Advance(7);
        Assert.Equal(visits.GetSettlement("fallowfield").Position, party.Status.Position);
        Assert.Null(party.Status.Destination);
        Assert.True(visits.CanEnter("fallowfield"));
        Assert.Null(visits.CurrentSettlementId);
    }

    [Fact]
    public void EnteringDuringTravelStopsThePartyAndClearsTheRenderedSegment()
    {
        var (party, session, visits) = Create(new Vector2(250, 465));
        party.TrySetDestination(new Vector2(900, 465));
        session.Advance(0.15);
        var position = party.Status.Position;
        Assert.True(visits.TryEnter("fallowfield"));
        Assert.Equal(position, party.Status.Position);
        Assert.Equal(position, party.Status.PreviousPosition);
        Assert.Null(party.Status.Destination);
        visits.TryLeave();
        session.Advance(5);
        Assert.Equal(position, party.Status.Position);
    }

    [Fact]
    public void UnknownCommandsAndLeavingOutsideAVisitDoNotChangeState()
    {
        var (party, session, visits) = Create(ValleyScenario.PlayerStart);
        var beforeParty = party.Status;
        var beforeTime = session.Status;
        Assert.False(visits.CanEnter("unknown"));
        Assert.False(visits.TryTravelTo("unknown"));
        Assert.False(visits.TryEnter("unknown"));
        Assert.False(visits.TryLeave());
        Assert.Throws<KeyNotFoundException>(() => visits.GetSettlement("unknown"));
        Assert.Equal(beforeParty, party.Status);
        Assert.Equal(beforeTime, session.Status);
    }

    [Fact]
    public void SettlementDefinitionsAreCopiedAndExposedReadOnly()
    {
        var (party, session, _) = Create(ValleyScenario.PlayerStart);
        var map = ValleyScenario.CreateMap();
        var definitions = map.Settlements.ToArray();
        var visits = new SettlementVisits(definitions, map.Size, party, session);
        definitions[0] = definitions[0] with { Position = new Vector2(0) };
        Assert.Equal(new Vector2(250, 465), visits.GetSettlement("fallowfield").Position);
        var settlements = Assert.IsAssignableFrom<IList<SettlementDefinition>>(visits.Settlements);
        Assert.Throws<NotSupportedException>(() => settlements[0] = definitions[0]);
    }

    [Fact]
    public void DuplicateSettlementIdsAreRejected()
    {
        var (party, session, _) = Create(ValleyScenario.PlayerStart);
        var map = ValleyScenario.CreateMap();
        Assert.Throws<ArgumentException>(() => new SettlementVisits(
            [map.Settlements[0], map.Settlements[0]], map.Size, party, session));
    }

    [Theory]
    [InlineData(-1, 465, 32)]
    [InlineData(1201, 465, 32)]
    [InlineData(250, 701, 32)]
    [InlineData(float.NaN, 465, 32)]
    [InlineData(250, float.PositiveInfinity, 32)]
    [InlineData(250, 465, 0)]
    [InlineData(250, 465, -1)]
    [InlineData(250, 465, float.NaN)]
    [InlineData(250, 465, float.PositiveInfinity)]
    public void InvalidLocationsAndEntryRadiiAreRejected(float x, float y, float radius)
    {
        var (party, session, _) = Create(ValleyScenario.PlayerStart);
        Assert.Throws<ArgumentException>(() => new SettlementVisits(
            [new SettlementDefinition("village", "Village", "Description", new Vector2(x, y), radius)],
            new Vector2(1200, 700), party, session));
    }
}
