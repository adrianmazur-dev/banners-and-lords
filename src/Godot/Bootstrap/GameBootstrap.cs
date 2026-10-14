using Godot;
using Game.Godot.Features.Campaign;
using Game.Godot.Features.Dynasties;
using Game.Godot.Features.Parties;
using Game.Godot.Features.WorldMap;
using Game.Simulation.Features.Campaign.Application;
using Game.Simulation.Features.Dynasties.Application;
using Game.Simulation.Features.Dynasties.Contracts;
using Game.Simulation.Features.Parties.Application;
using Game.Simulation.Features.Parties.Contracts;
using Game.Simulation.Features.WorldMap.Application;

namespace Game.Godot.Bootstrap;

public partial class GameBootstrap : Node
{
    [Export] public CampaignRunner Runner { get; set; } = null!;
    [Export] public CampaignController Campaign { get; set; } = null!;
    [Export] public ValleyMapController Map { get; set; } = null!;
    [Export] public PartyPanelController Party { get; set; } = null!;
    [Export] public DynastyPanelController Dynasty { get; set; } = null!;

    private CampaignSession _session = null!;
    private IDynastyQuery _dynasties = null!;

    public override void _Ready()
    {
        var map = ValleyScenario.CreateMap();
        _dynasties = StartingDynasty.CreateRoster();
        var party = new PartyTravel(new PartyIdentity("player-company", StartingDynasty.PlayerCharacterId),
            _dynasties, map.Size, ValleyScenario.PlayerStart);
        _session = new CampaignSession(systems: [party]);
        Runner.Initialize(_session);
        Campaign.Initialize(_session);
        Map.Initialize(map, party, _session);
        Party.Initialize(party, _dynasties, _session);
        Dynasty.Initialize(StartingDynasty.PlayerCharacterId, _dynasties);
    }
}
