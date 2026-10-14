using Godot;
using Game.Godot.Features.Campaign;
using Game.Simulation.Features.Campaign.Application;

namespace Game.Godot.Bootstrap;

public partial class GameBootstrap : Node
{
    [Export] public CampaignRunner Runner { get; set; } = null!;
    [Export] public CampaignController Campaign { get; set; } = null!;

    private readonly CampaignSession _session = new();

    public override void _Ready()
    {
        Runner.Initialize(_session);
        Campaign.Initialize(_session);
    }
}
