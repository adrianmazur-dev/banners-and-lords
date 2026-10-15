using Godot;
using Game.Godot.Features.Campaign;
using Game.Godot.Features.Dynasties;
using Game.Godot.Features.Inventory;
using Game.Godot.Features.Parties;
using Game.Godot.Features.Settlements;
using Game.Godot.Features.WorldMap;
using Game.Simulation.Features.Campaign.Application;
using Game.Simulation.Features.Dynasties.Application;
using Game.Simulation.Features.Dynasties.Contracts;
using Game.Simulation.Features.Inventory.Application;
using Game.Simulation.Features.Inventory.Contracts;
using Game.Simulation.Features.Parties.Application;
using Game.Simulation.Features.Parties.Contracts;
using Game.Simulation.Features.Settlements.Application;
using Game.Simulation.Features.Settlements.Contracts;
using Game.Simulation.Features.WorldMap.Application;

namespace Game.Godot.Bootstrap;

public partial class GameBootstrap : Node
{
    [Export] public CampaignRunner Runner { get; set; } = null!;
    [Export] public CampaignController Campaign { get; set; } = null!;
    [Export] public ValleyMapController Map { get; set; } = null!;
    [Export] public PartyPanelController Party { get; set; } = null!;
    [Export] public DynastyPanelController Dynasty { get; set; } = null!;
    [Export] public CampaignPanelsController Panels { get; set; } = null!;
    [Export] public SettlementPanelController Settlement { get; set; } = null!;
    [Export] public InventoryPanelController Inventory { get; set; } = null!;
    [Export] public Button InventoryButton { get; set; } = null!;

    private CampaignSession _session = null!;
    private IDynastyQuery _dynasties = null!;
    private ISettlementVisits _visits = null!;
    private IPartyInventory _inventory = null!;

    public override void _Ready()
    {
        var map = ValleyScenario.CreateMap();
        _dynasties = StartingDynasty.CreateRoster();
        var party = new PartyTravel(new PartyIdentity("player-company", StartingDynasty.PlayerCharacterId),
            _dynasties, map.Size, ValleyScenario.PlayerStart);
        var items = StartingInventory.CreateCatalog();
        _inventory = StartingInventory.CreateFor(party.Identity, items);
        _session = new CampaignSession(systems: [party]);
        _visits = new SettlementVisits(map.Settlements, map.Size, party, _session);
        Runner.Initialize(_session);
        Campaign.Initialize(_session);
        Map.Initialize(map, party, _session, _visits);
        Party.Initialize(party, _dynasties, _session);
        Dynasty.Initialize(StartingDynasty.PlayerCharacterId, _dynasties);
        Settlement.Initialize(_visits);
        var commander = _dynasties.GetCharacter(party.Identity.CommanderId);
        Inventory.Initialize(_session, _inventory, items, commander.Name, _dynasties.GetDynasty(commander.DynastyId).Name, InventoryButton);
        Inventory.Opened += Panels.Close;
        Settlement.TravelStarted += Panels.Close;
        Map.SettlementSelected += ShowSettlement;
    }

    public override void _ExitTree()
    {
        Map.SettlementSelected -= ShowSettlement;
        Inventory.Opened -= Panels.Close;
        Settlement.TravelStarted -= Panels.Close;
    }

    private void ShowSettlement(string settlementId)
    {
        Settlement.SelectSettlement(settlementId);
        Panels.OpenSettlement();
    }
}
