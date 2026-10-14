namespace Game.Simulation.Features.Dynasties.Contracts;

public interface IDynastyQuery
{
    CharacterInfo GetCharacter(string characterId);
    DynastyInfo GetDynasty(string dynastyId);
    IReadOnlyList<CharacterInfo> GetMembers(string dynastyId);
}
