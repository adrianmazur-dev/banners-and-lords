using Godot;
using Game.Simulation.Features.Campaign.Contracts;

namespace Game.Godot.Features.Campaign;

public partial class CampaignController : Control
{
    [Export] public Label ClockLabel { get; set; } = null!;
    [Export] public Label StateLabel { get; set; } = null!;
    [Export] public Button PauseButton { get; set; } = null!;
    [Export] public Button NormalSpeedButton { get; set; } = null!;
    [Export] public Button FastSpeedButton { get; set; } = null!;

    private ICampaignSession? _session;
    private CampaignStatus? _lastStatus;

    public void Initialize(ICampaignSession session)
    {
        _session = session;
        Refresh(force: true);
    }

    public override void _Ready()
    {
        PauseButton.Pressed += TogglePause;
        NormalSpeedButton.Pressed += UseNormalSpeed;
        FastSpeedButton.Pressed += UseFastSpeed;
    }

    public override void _Process(double delta)
    {
        Refresh();
    }

    public override void _Input(InputEvent @event)
    {
        if (_session is null)
        {
            return;
        }

        if (@event.IsActionPressed("campaign_pause", exactMatch: true))
        {
            TogglePause();
        }
        else if (@event.IsActionPressed("campaign_speed_normal", exactMatch: true))
        {
            UseNormalSpeed();
        }
        else if (@event.IsActionPressed("campaign_speed_fast", exactMatch: true))
        {
            UseFastSpeed();
        }
        else
        {
            return;
        }

        // Consume global time shortcuts before focused buttons can treat Space as ui_accept.
        GetViewport().SetInputAsHandled();
    }

    public override void _ExitTree()
    {
        PauseButton.Pressed -= TogglePause;
        NormalSpeedButton.Pressed -= UseNormalSpeed;
        FastSpeedButton.Pressed -= UseFastSpeed;
    }

    private void TogglePause()
    {
        if (_session is null)
        {
            return;
        }

        _session.SetPaused(!_session.Status.IsPaused);
        Refresh(force: true);
    }

    private void UseNormalSpeed() => SetSpeed(CampaignSpeed.Normal);
    private void UseFastSpeed() => SetSpeed(CampaignSpeed.Fast);

    private void SetSpeed(CampaignSpeed speed)
    {
        _session?.SetSpeed(speed);
        Refresh(force: true);
    }

    private void Refresh(bool force = false)
    {
        if (_session is null)
        {
            return;
        }

        var status = _session.Status;
        if (!force && status == _lastStatus)
        {
            return;
        }

        ClockLabel.Text = $"Day {status.Day}  ·  {status.Hour:00}:{status.Minute:00}";
        StateLabel.Text = $"{(status.IsPaused ? "Paused" : "Running")}  ·  ×{(int)status.Speed}";
        PauseButton.Text = status.IsPaused ? "Resume" : "Pause";
        NormalSpeedButton.SetPressedNoSignal(status.Speed == CampaignSpeed.Normal);
        FastSpeedButton.SetPressedNoSignal(status.Speed == CampaignSpeed.Fast);
        _lastStatus = status;
    }
}
