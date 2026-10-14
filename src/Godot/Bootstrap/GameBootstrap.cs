using Godot;
using Game.Godot.Features.Campaign;
using Game.Godot.Features.WorldMap;
using Game.Simulation.Features.Campaign.Application;
using Game.Simulation.Features.Parties.Application;
using Game.Simulation.Features.WorldMap.Application;

namespace Game.Godot.Bootstrap;

public partial class GameBootstrap : Node
{
    [Export] public CampaignRunner Runner { get; set; } = null!;
    [Export] public CampaignController Campaign { get; set; } = null!;
    [Export] public ValleyMapController Map { get; set; } = null!;

    private CampaignSession _session = null!;

    public override void _Ready()
    {
        var map = ValleyScenario.CreateMap();
        var party = new PartyTravel(map.Size, ValleyScenario.PlayerStart);
        _session = new CampaignSession(systems: [party]);
        Runner.Initialize(_session);
        Campaign.Initialize(_session);
        Map.Initialize(map, party, _session);
    }
}
