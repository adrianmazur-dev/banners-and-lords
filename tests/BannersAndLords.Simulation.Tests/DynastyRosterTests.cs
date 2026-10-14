using System.Numerics;
using Game.Simulation.Features.Dynasties.Application;
using Game.Simulation.Features.Dynasties.Contracts;
using Game.Simulation.Features.Parties.Application;
using Game.Simulation.Features.Parties.Contracts;

namespace BannersAndLords.Simulation.Tests;

public sealed class DynastyRosterTests
{
    private static DynastyInfo[] Houses() =>
    [
        new("ashford", "House Ashford", "elinor"),
        new("blackthorn", "House Blackthorn", "oswin")
    ];

    private static CharacterInfo[] Characters() =>
    [
        new("rowan", "Rowan Ashford", "ashford"),
        new("elinor", "Elinor Ashford", "ashford"),
        new("oswin", "Oswin Blackthorn", "blackthorn"),
        new("aldric", "Aldric Ashford", "ashford")
    ];

    [Fact]
    public void MembersAreGroupedByDynastyInStableIdOrder()
    {
        var roster = new DynastyRoster(Houses(), Characters());
        Assert.Equal(["aldric", "elinor", "rowan"], roster.GetMembers("ashford").Select(member => member.Id));
        Assert.Equal("oswin", Assert.Single(roster.GetMembers("blackthorn")).Id);
        var house = roster.GetDynasty("ashford");
        Assert.Equal(house.Id, roster.GetCharacter(house.HeadId).DynastyId);
    }

    [Fact]
    public void DuplicateDynastyIdsAreRejected()
    {
        var houses = Houses();
        Assert.Throws<ArgumentException>(() => new DynastyRoster([houses[0], houses[0]], Characters()));
    }

    [Fact]
    public void DuplicateCharacterIdsAreRejected()
    {
        var characters = Characters();
        Assert.Throws<ArgumentException>(() => new DynastyRoster(Houses(), [.. characters, characters[0]]));
    }

    [Fact]
    public void CharacterCannotReferenceAnUnknownDynasty()
    {
        var characters = Characters();
        characters[0] = characters[0] with { DynastyId = "unknown" };
        Assert.Throws<ArgumentException>(() => new DynastyRoster(Houses(), characters));
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("oswin")]
    public void HeadMustExistAndBelongToTheDynasty(string headId)
    {
        var houses = Houses();
        houses[0] = houses[0] with { HeadId = headId };
        Assert.Throws<ArgumentException>(() => new DynastyRoster(houses, Characters()));
    }

    [Fact]
    public void BlankNamesAndIdsAreRejected()
    {
        var houses = Houses();
        var characters = Characters();
        Assert.Throws<ArgumentException>(() => new DynastyRoster([houses[0] with { Id = " " }], characters));
        Assert.Throws<ArgumentException>(() => new DynastyRoster([houses[0] with { Name = "" }], characters));
        Assert.Throws<ArgumentException>(() => new DynastyRoster([houses[0] with { HeadId = " " }], characters));
        Assert.Throws<ArgumentException>(() => new DynastyRoster(houses, [characters[0] with { Id = "" }]));
        Assert.Throws<ArgumentException>(() => new DynastyRoster(houses, [characters[0] with { Name = " " }]));
        Assert.Throws<ArgumentException>(() => new DynastyRoster(houses, [characters[0] with { DynastyId = "" }]));
    }

    [Fact]
    public void SourceArraysAndMemberQueriesCannotMutateTheRoster()
    {
        var houses = Houses();
        var characters = Characters();
        var roster = new DynastyRoster(houses, characters);
        houses[0] = houses[0] with { HeadId = "unknown" };
        characters[0] = characters[0] with { DynastyId = "blackthorn" };
        Assert.Equal("elinor", roster.GetDynasty("ashford").HeadId);
        Assert.Equal("ashford", roster.GetCharacter("rowan").DynastyId);
        var members = Assert.IsAssignableFrom<IList<CharacterInfo>>(roster.GetMembers("ashford"));
        Assert.Throws<NotSupportedException>(() => members[0] = characters[0]);
    }

    [Fact]
    public void UnknownQueriesDoNotReturnAnotherCharacterOrDynasty()
    {
        var roster = new DynastyRoster(Houses(), Characters());
        Assert.Throws<KeyNotFoundException>(() => roster.GetCharacter("unknown"));
        Assert.Throws<KeyNotFoundException>(() => roster.GetDynasty("unknown"));
        Assert.Throws<KeyNotFoundException>(() => roster.GetMembers("unknown"));
    }

    [Fact]
    public void DifferentMembersCanCommandSeparatePartiesWithoutBeingTheHead()
    {
        var roster = new DynastyRoster(Houses(), Characters());
        var first = new PartyTravel(new PartyIdentity("first", "aldric"), roster, new Vector2(1200, 700), new Vector2(100));
        var second = new PartyTravel(new PartyIdentity("second", "rowan"), roster, new Vector2(1200, 700), new Vector2(200));
        first.TrySetDestination(new Vector2(500, 100));
        first.AdvanceTick(1, 1);
        Assert.Equal(new PartyIdentity("first", "aldric"), first.Identity);
        Assert.Equal(new PartyIdentity("second", "rowan"), second.Identity);
        Assert.Equal(new Vector2(200), second.Status.Position);
        Assert.Equal("elinor", roster.GetDynasty("ashford").HeadId);
        Assert.All(new[] { first, second }, party =>
            Assert.Equal("ashford", roster.GetCharacter(party.Identity.CommanderId).DynastyId));
    }

    [Fact]
    public void PartyCannotReferenceAnUnknownCommander()
    {
        var roster = new DynastyRoster(Houses(), Characters());
        Assert.Throws<KeyNotFoundException>(() => new PartyTravel(new PartyIdentity("party", "unknown"),
            roster, new Vector2(1200, 700), new Vector2(100)));
    }

    [Theory]
    [InlineData("", "aldric")]
    [InlineData("party", " ")]
    public void PartyRequiresStableIdentityAndCommanderIds(string partyId, string commanderId)
    {
        var roster = new DynastyRoster(Houses(), Characters());
        Assert.Throws<ArgumentException>(() => new PartyTravel(new PartyIdentity(partyId, commanderId),
            roster, new Vector2(1200, 700), new Vector2(100)));
    }

    [Fact]
    public void StartingScenarioHasOneHouseWithThreeMembersIncludingThePlayerAndHead()
    {
        var roster = StartingDynasty.CreateRoster();
        var player = roster.GetCharacter(StartingDynasty.PlayerCharacterId);
        var house = roster.GetDynasty(player.DynastyId);
        var members = roster.GetMembers(house.Id);
        Assert.Equal(3, members.Count);
        Assert.Contains(player, members);
        Assert.Contains(roster.GetCharacter(house.HeadId), members);
    }
}
