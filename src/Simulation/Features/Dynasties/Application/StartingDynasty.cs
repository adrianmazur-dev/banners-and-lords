using Game.Simulation.Features.Dynasties.Contracts;

namespace Game.Simulation.Features.Dynasties.Application;

public static class StartingDynasty
{
    public const string PlayerCharacterId = "aldric-ashford";

    public static IDynastyQuery CreateRoster() => new DynastyRoster(
        [new DynastyInfo("ashford", "House Ashford", "elinor-ashford")],
        [
            new CharacterInfo(PlayerCharacterId, "Aldric Ashford", "ashford"),
            new CharacterInfo("elinor-ashford", "Elinor Ashford", "ashford"),
            new CharacterInfo("rowan-ashford", "Rowan Ashford", "ashford")
        ]);
}
