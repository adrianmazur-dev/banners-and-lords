using System.Collections.Generic;
using System.Linq;
using Godot;
using Game.Simulation.Features.Dynasties.Contracts;

namespace Game.Godot.Features.Dynasties;

public partial class DynastyPanelController : PanelContainer
{
    [Export] public Label DynastyLabel { get; set; } = null!;
    [Export] public Label HeadLabel { get; set; } = null!;
    [Export] public Label MembersHeading { get; set; } = null!;
    [Export] public Label MembersLabel { get; set; } = null!;

    public void Initialize(string playerCharacterId, IDynastyQuery dynasties)
    {
        var player = dynasties.GetCharacter(playerCharacterId);
        var dynasty = dynasties.GetDynasty(player.DynastyId);
        var members = dynasties.GetMembers(dynasty.Id);
        DynastyLabel.Text = dynasty.Name;
        HeadLabel.Text = dynasties.GetCharacter(dynasty.HeadId).Name;
        MembersHeading.Text = $"MEMBERS · {members.Count}";
        MembersLabel.Text = string.Join("\n", members.Select(member =>
        {
            var roles = new List<string>();
            if (member.Id == dynasty.HeadId) roles.Add("Head");
            if (member.Id == playerCharacterId) roles.Add("You");
            return roles.Count > 0 ? $"{member.Name} · {string.Join(", ", roles)}" : member.Name;
        }));
    }
}
