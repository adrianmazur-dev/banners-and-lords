using Godot;
using Game.Simulation.Features.Campaign.Contracts;
using Game.Simulation.Features.Dynasties.Contracts;
using Game.Simulation.Features.Parties.Contracts;

namespace Game.Godot.Features.Parties;

public partial class PartyPanelController : PanelContainer
{
    [Export] public Button CommanderButton { get; set; } = null!;
    [Export] public Label PartyLabel { get; set; } = null!;
    [Export] public Label StateLabel { get; set; } = null!;
    [Export] public Label OrderLabel { get; set; } = null!;

    private IPartyTravel? _party;
    private ICampaignSession? _session;

    public void Initialize(IPartyTravel party, IDynastyQuery dynasties, ICampaignSession session)
    {
        _party = party;
        _session = session;
        var commander = dynasties.GetCharacter(party.Identity.CommanderId);
        CommanderButton.Text = commander.Name;
        PartyLabel.Text = $"{commander.Name}'s party";
        RefreshTravel();
    }

    public override void _Process(double delta)
    {
        if (Visible)
        {
            RefreshTravel();
        }
    }

    private void RefreshTravel()
    {
        if (_party is null || _session is null)
        {
            return;
        }

        var hasOrder = _party.Status.Destination.HasValue;
        StateLabel.Text = hasOrder
            ? _session.Status.IsPaused ? "Paused" : "Traveling"
            : "Holding position";
        OrderLabel.Text = hasOrder ? "Travel to marked destination" : "No movement order";
    }
}
