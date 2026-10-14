using Game.Simulation.Features.Dynasties.Contracts;

namespace Game.Simulation.Features.Dynasties.Application;

public sealed class DynastyRoster : IDynastyQuery
{
    private readonly Dictionary<string, CharacterInfo> _characters = new(StringComparer.Ordinal);
    private readonly Dictionary<string, DynastyInfo> _dynasties = new(StringComparer.Ordinal);
    private readonly Dictionary<string, IReadOnlyList<CharacterInfo>> _members = new(StringComparer.Ordinal);

    public DynastyRoster(IEnumerable<DynastyInfo> dynasties, IEnumerable<CharacterInfo> characters)
    {
        foreach (var dynasty in dynasties)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(dynasty.Id);
            ArgumentException.ThrowIfNullOrWhiteSpace(dynasty.Name);
            ArgumentException.ThrowIfNullOrWhiteSpace(dynasty.HeadId);
            if (!_dynasties.TryAdd(dynasty.Id, dynasty))
            {
                throw new ArgumentException($"Duplicate dynasty ID '{dynasty.Id}'.", nameof(dynasties));
            }
        }

        foreach (var character in characters)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(character.Id);
            ArgumentException.ThrowIfNullOrWhiteSpace(character.Name);
            ArgumentException.ThrowIfNullOrWhiteSpace(character.DynastyId);
            if (!_dynasties.ContainsKey(character.DynastyId))
            {
                throw new ArgumentException($"Character '{character.Id}' references an unknown dynasty.", nameof(characters));
            }
            if (!_characters.TryAdd(character.Id, character))
            {
                throw new ArgumentException($"Duplicate character ID '{character.Id}'.", nameof(characters));
            }
        }

        foreach (var dynasty in _dynasties.Values)
        {
            if (!_characters.TryGetValue(dynasty.HeadId, out var head) || head.DynastyId != dynasty.Id)
            {
                throw new ArgumentException($"Dynasty '{dynasty.Id}' must have a head from its own members.", nameof(dynasties));
            }
            _members.Add(dynasty.Id, Array.AsReadOnly(_characters.Values
                .Where(character => character.DynastyId == dynasty.Id)
                .OrderBy(character => character.Id, StringComparer.Ordinal)
                .ToArray()));
        }
    }

    public CharacterInfo GetCharacter(string characterId) => _characters[characterId];
    public DynastyInfo GetDynasty(string dynastyId) => _dynasties[dynastyId];
    public IReadOnlyList<CharacterInfo> GetMembers(string dynastyId) => _members[dynastyId];
}
