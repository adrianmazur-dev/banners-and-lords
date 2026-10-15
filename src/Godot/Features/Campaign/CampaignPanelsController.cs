using System.Linq;
using Godot;

namespace Game.Godot.Features.Campaign;

public partial class CampaignPanelsController : Control
{
    [Export] public PanelContainer PartyPanel { get; set; } = null!;
    [Export] public PanelContainer DynastyPanel { get; set; } = null!;
    [Export] public PanelContainer SettlementPanel { get; set; } = null!;
    [Export] public Button PartyButton { get; set; } = null!;
    [Export] public Button DynastyButton { get; set; } = null!;
    [Export] public Button PartyCloseButton { get; set; } = null!;
    [Export] public Button DynastyCloseButton { get; set; } = null!;
    [Export] public Button CommanderButton { get; set; } = null!;
    [Export] public Button SettlementCloseButton { get; set; } = null!;
    [Export] public Control VisitView { get; set; } = null!;

    private PanelContainer? _active;

    public override void _Process(double delta)
    {
        if (_active is null || VisitView.Visible) return;
        var focused = GetViewport().GuiGetFocusOwner();
        if (focused is null || !_active.IsAncestorOf(focused)) FocusPanel();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_active is not null && !VisitView.Visible
            && @event is InputEventKey { Pressed: true, Echo: false, Keycode: Key.Escape })
        {
            Close();
            GetViewport().SetInputAsHandled();
        }
    }

    public override void _Ready()
    {
        PartyButton.Pressed += ToggleParty;
        DynastyButton.Pressed += ToggleDynasty;
        PartyCloseButton.Pressed += Close;
        DynastyCloseButton.Pressed += Close;
        CommanderButton.Pressed += OpenDynasty;
        SettlementCloseButton.Pressed += Close;
        ShowPanel(null);
    }

    public override void _ExitTree()
    {
        PartyButton.Pressed -= ToggleParty;
        DynastyButton.Pressed -= ToggleDynasty;
        PartyCloseButton.Pressed -= Close;
        DynastyCloseButton.Pressed -= Close;
        CommanderButton.Pressed -= OpenDynasty;
        SettlementCloseButton.Pressed -= Close;
    }

    private void ToggleParty() => ShowPanel(PartyPanel.Visible ? null : PartyPanel);
    private void ToggleDynasty() => ShowPanel(DynastyPanel.Visible ? null : DynastyPanel);
    private void OpenDynasty() => ShowPanel(DynastyPanel);
    public void OpenSettlement() => ShowPanel(SettlementPanel);
    public void Close() => ShowPanel(null);

    private void ShowPanel(PanelContainer? panel)
    {
        var previous = _active;
        _active = panel;
        PartyPanel.Visible = panel == PartyPanel;
        DynastyPanel.Visible = panel == DynastyPanel;
        SettlementPanel.Visible = panel == SettlementPanel;
        PartyButton.SetPressedNoSignal(PartyPanel.Visible);
        DynastyButton.SetPressedNoSignal(DynastyPanel.Visible);
        if (panel is not null)
        {
            var buttons = panel.FindChildren("*", "Button", true, false).OfType<Button>().ToArray();
            for (var index = 0; index < buttons.Length; index++)
            {
                var button = buttons[index];
                var before = button.GetPathTo(buttons[(index + buttons.Length - 1) % buttons.Length]);
                var after = button.GetPathTo(buttons[(index + 1) % buttons.Length]);
                button.FocusPrevious = before;
                button.FocusNext = after;
                button.FocusNeighborLeft = before;
                button.FocusNeighborTop = before;
                button.FocusNeighborRight = after;
                button.FocusNeighborBottom = after;
            }
            FocusPanel();
        }
        else if (previous is not null)
        {
            (previous == DynastyPanel ? DynastyButton : PartyButton).GrabFocus();
        }
    }

    private void FocusPanel() => (_active == PartyPanel ? PartyCloseButton
        : _active == DynastyPanel ? DynastyCloseButton : SettlementCloseButton).GrabFocus();
}
