using System;
using Godot;
using Game.Simulation.Features.Settlements.Contracts;

namespace Game.Godot.Features.Settlements;

public partial class SettlementPanelController : PanelContainer
{
    [Export] public Label NameLabel { get; set; } = null!;
    [Export] public Label DescriptionLabel { get; set; } = null!;
    [Export] public Label AccessLabel { get; set; } = null!;
    [Export] public Button TravelButton { get; set; } = null!;
    [Export] public Button EnterButton { get; set; } = null!;
    [Export] public Control VisitView { get; set; } = null!;
    [Export] public Label VisitNameLabel { get; set; } = null!;
    [Export] public Label VisitDescriptionLabel { get; set; } = null!;
    [Export] public Button LeaveButton { get; set; } = null!;

    public event Action? TravelStarted;

    private ISettlementVisits? _visits;
    private string? _selectedId;
    private string? _displayedVisitId;

    public void Initialize(ISettlementVisits visits)
    {
        _visits = visits;
        Refresh();
    }

    public void SelectSettlement(string settlementId)
    {
        if (_visits is null || _visits.CurrentSettlementId is not null)
        {
            return;
        }
        var settlement = _visits.GetSettlement(settlementId);
        _selectedId = settlement.Id;
        NameLabel.Text = settlement.Name;
        DescriptionLabel.Text = settlement.Description;
        Refresh();
    }

    public override void _Ready()
    {
        TravelButton.Pressed += Travel;
        EnterButton.Pressed += Enter;
        LeaveButton.Pressed += Leave;
    }

    public override void _ExitTree()
    {
        TravelButton.Pressed -= Travel;
        EnterButton.Pressed -= Enter;
        LeaveButton.Pressed -= Leave;
    }

    public override void _Process(double delta) => Refresh();

    private void Travel()
    {
        if (_selectedId is not null)
        {
            if (_visits?.TryTravelTo(_selectedId) == true) TravelStarted?.Invoke();
            Refresh();
        }
    }

    private void Enter()
    {
        if (_selectedId is not null)
        {
            _visits?.TryEnter(_selectedId);
            Refresh();
        }
    }

    private void Leave()
    {
        _visits?.TryLeave();
        Refresh();
    }

    private void Refresh()
    {
        if (_visits is null)
        {
            return;
        }
        var current = _visits.CurrentSettlementId;
        var visitChanged = current != _displayedVisitId;
        VisitView.Visible = current is not null;
        if (current is not null && visitChanged)
        {
            var settlement = _visits.GetSettlement(current);
            VisitNameLabel.Text = settlement.Name;
            VisitDescriptionLabel.Text = settlement.Description;
        }
        _displayedVisitId = current;
        var canEnter = _selectedId is not null && _visits.CanEnter(_selectedId);
        EnterButton.Disabled = !canEnter;
        TravelButton.Disabled = _selectedId is null || current is not null;
        AccessLabel.Text = current is not null ? "Your party is visiting this village."
            : canEnter ? "Your party is nearby." : "Move your party closer to enter.";
        if (visitChanged)
        {
            if (current is not null)
            {
                LeaveButton.GrabFocus();
            }
            else if (Visible && !EnterButton.Disabled)
            {
                EnterButton.GrabFocus();
            }
        }
    }
}
