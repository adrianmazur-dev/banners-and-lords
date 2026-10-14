using Godot;

namespace Game.Godot.Features.Campaign;

public partial class CampaignPanelsController : Control
{
    [Export] public PanelContainer PartyPanel { get; set; } = null!;
    [Export] public PanelContainer DynastyPanel { get; set; } = null!;
    [Export] public Button PartyButton { get; set; } = null!;
    [Export] public Button DynastyButton { get; set; } = null!;
    [Export] public Button PartyCloseButton { get; set; } = null!;
    [Export] public Button DynastyCloseButton { get; set; } = null!;
    [Export] public Button CommanderButton { get; set; } = null!;

    public override void _Ready()
    {
        PartyButton.Pressed += ToggleParty;
        DynastyButton.Pressed += ToggleDynasty;
        PartyCloseButton.Pressed += Close;
        DynastyCloseButton.Pressed += Close;
        CommanderButton.Pressed += OpenDynasty;
        ShowPanel(null);
    }

    public override void _ExitTree()
    {
        PartyButton.Pressed -= ToggleParty;
        DynastyButton.Pressed -= ToggleDynasty;
        PartyCloseButton.Pressed -= Close;
        DynastyCloseButton.Pressed -= Close;
        CommanderButton.Pressed -= OpenDynasty;
    }

    private void ToggleParty() => ShowPanel(PartyPanel.Visible ? null : PartyPanel);
    private void ToggleDynasty() => ShowPanel(DynastyPanel.Visible ? null : DynastyPanel);
    private void OpenDynasty() => ShowPanel(DynastyPanel);
    private void Close() => ShowPanel(null);

    private void ShowPanel(PanelContainer? panel)
    {
        PartyPanel.Visible = panel == PartyPanel;
        DynastyPanel.Visible = panel == DynastyPanel;
        PartyButton.SetPressedNoSignal(PartyPanel.Visible);
        DynastyButton.SetPressedNoSignal(DynastyPanel.Visible);
    }
}
