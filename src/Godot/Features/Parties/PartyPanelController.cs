using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;
using Game.Simulation.Features.Campaign.Contracts;
using Game.Simulation.Features.Dynasties.Contracts;
using Game.Simulation.Features.Inventory.Contracts;
using Game.Simulation.Features.Parties.Contracts;

namespace Game.Godot.Features.Parties;

public partial class PartyPanelController : PanelContainer
{
    [Export] public Button CommanderButton { get; set; } = null!;
    [Export] public Label PartyLabel { get; set; } = null!;
    [Export] public Label StateLabel { get; set; } = null!;
    [Export] public Label OrderLabel { get; set; } = null!;
    [Export] public Tree CargoList { get; set; } = null!;
    [Export] public Label EmptyCargoLabel { get; set; } = null!;

    private IPartyTravel? _party;
    private ICampaignSession? _session;
    private IPartyInventory? _inventory;
    private IGoodsCatalog? _goods;
    private IReadOnlyList<InventoryStack>? _displayedCargo;

    public override void _Ready()
    {
        CargoList.SetColumnTitle(0, "Item");
        CargoList.SetColumnTitle(1, "Qty");
        CargoList.SetColumnExpand(1, false);
        CargoList.SetColumnCustomMinimumWidth(1, 52);
    }

    public void Initialize(IPartyTravel party, IDynastyQuery dynasties, ICampaignSession session,
        IPartyInventory inventory, IGoodsCatalog goods)
    {
        if (inventory.PartyId != party.Identity.Id)
        {
            throw new ArgumentException("The inventory must belong to the displayed party.", nameof(inventory));
        }
        _party = party;
        _session = session;
        _inventory = inventory;
        _goods = goods;
        _displayedCargo = null;
        var commander = dynasties.GetCharacter(party.Identity.CommanderId);
        CommanderButton.Text = commander.Name;
        PartyLabel.Text = $"{commander.Name}'s party";
        RefreshTravel();
        RefreshCargo();
    }

    public override void _Process(double delta)
    {
        if (Visible)
        {
            RefreshTravel();
            RefreshCargo();
        }
    }

    private void RefreshTravel()
    {
        if (_party is null || _session is null)
        {
            return;
        }

        var hasOrder = _party.Status.Destination.HasValue;
        StateLabel.Text = hasOrder
            ? _session.Status.IsPaused ? "Paused" : "Traveling"
            : "Holding position";
        OrderLabel.Text = hasOrder ? "Travel to marked destination" : "No movement order";
    }

    private void RefreshCargo()
    {
        if (_inventory is null || _goods is null || ReferenceEquals(_displayedCargo, _inventory.Stacks))
        {
            return;
        }
        var stacks = _inventory.Stacks;
        CargoList.Clear();
        var root = CargoList.CreateItem();
        foreach (var stack in stacks)
        {
            var goods = _goods.GetDefinition(stack.GoodsId);
            var item = CargoList.CreateItem(root);
            item.SetText(0, goods.Name);
            item.SetText(1, stack.Quantity.ToString(CultureInfo.InvariantCulture));
            item.SetTextAlignment(1, HorizontalAlignment.Right);
            item.SetTooltipText(0, goods.Category switch
            {
                GoodsCategory.RawMaterial => "Raw material",
                GoodsCategory.Manufactured => "Manufactured goods",
                _ => "Food"
            });
        }
        EmptyCargoLabel.Visible = stacks.Count == 0;
        CargoList.Visible = stacks.Count > 0;
        _displayedCargo = stacks;
    }
}
