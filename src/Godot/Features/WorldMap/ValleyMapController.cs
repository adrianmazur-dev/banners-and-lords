using Godot;
using Game.Simulation.Features.Campaign.Contracts;
using Game.Simulation.Features.Parties.Contracts;
using Game.Simulation.Features.Settlements.Contracts;
using Game.Simulation.Features.WorldMap.Contracts;
using MapPosition = System.Numerics.Vector2;

namespace Game.Godot.Features.WorldMap;

public partial class ValleyMapController : Control
{
    [Signal] public delegate void SettlementSelectedEventHandler(string settlementId);

    [Export] public Node2D WorldCanvas { get; set; } = null!;
    [Export] public ValleyTerrain Terrain { get; set; } = null!;
    [Export] public PartyMapView PartyView { get; set; } = null!;
    [Export] public Label OrdersLabel { get; set; } = null!;

    private WorldMapDefinition? _map;
    private IPartyTravel? _party;
    private ICampaignSession? _session;
    private ISettlementVisits? _visits;

    public void Initialize(WorldMapDefinition map, IPartyTravel party, ICampaignSession session, ISettlementVisits visits)
    {
        _map = map;
        _party = party;
        _session = session;
        _visits = visits;
        Terrain.Initialize(map);
        PartyView.Initialize(party, session);
        FitMap();
        RefreshOrders();
    }

    public override void _Ready() => Resized += FitMap;

    public override void _ExitTree() => Resized -= FitMap;

    public override void _Process(double delta) => RefreshOrders();

    public override void _GuiInput(InputEvent @event)
    {
        if (_party is null || _map is null || _visits?.CurrentSettlementId is not null
            || @event is not InputEventMouseButton { Pressed: true } mouse)
        {
            return;
        }

        var position = (mouse.Position - WorldCanvas.Position) / WorldCanvas.Scale;
        if (mouse.ButtonIndex == MouseButton.Left)
        {
            foreach (var settlement in _map.Settlements)
            {
                var origin = new Vector2(settlement.Position.X, settlement.Position.Y);
                if (new Rect2(origin + new Vector2(-82, -64), new Vector2(150, 126)).HasPoint(position))
                {
                    EmitSignal(SignalName.SettlementSelected, settlement.Id);
                    AcceptEvent();
                    return;
                }
            }
        }
        else if (mouse.ButtonIndex == MouseButton.Right && _party.TrySetDestination(new MapPosition(position.X, position.Y)))
        {
            RefreshOrders();
            AcceptEvent();
        }
    }

    private void FitMap()
    {
        if (_map is null || Size.X <= 0 || Size.Y <= 0)
        {
            return;
        }

        var worldSize = new Vector2(_map.Size.X, _map.Size.Y);
        var scale = Mathf.Min(Size.X / worldSize.X, Size.Y / worldSize.Y);
        WorldCanvas.Scale = Vector2.One * scale;
        WorldCanvas.Position = (Size - worldSize * scale) / 2;
    }

    private void RefreshOrders()
    {
        if (_party is null || _session is null)
        {
            return;
        }

        var moving = _party.Status.Destination.HasValue || PartyView.IsTraveling;
        OrdersLabel.Text = moving
            ? _session.Status.IsPaused ? "Move order ready · paused" : "Traveling"
            : "Holding position";
    }
}
