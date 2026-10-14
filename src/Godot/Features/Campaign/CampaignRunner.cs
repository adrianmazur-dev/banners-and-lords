using Godot;
using Game.Simulation.Features.Campaign.Contracts;

namespace Game.Godot.Features.Campaign;

public partial class CampaignRunner : Node
{
    private ICampaignSession? _session;

    public void Initialize(ICampaignSession session) => _session = session;

    public override void _Process(double delta) => _session?.Advance(delta);
}
