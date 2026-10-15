using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Game.Simulation.Features.Campaign.Contracts;
using Game.Simulation.Features.Inventory.Contracts;

namespace Game.Godot.Features.Inventory;

public partial class InventoryPanelController : Control
{
    [Export] public Button CloseButton { get; set; } = null!;
    [Export] public HBoxContainer CategoryBar { get; set; } = null!;
    [Export] public VBoxContainer ItemList { get; set; } = null!;
    [Export] public Control CharacterStage { get; set; } = null!;
    [Export] public Label CharacterName { get; set; } = null!;
    [Export] public Label HouseName { get; set; } = null!;
    [Export] public Label CategoryHeading { get; set; } = null!;
    [Export] public Label ItemCount { get; set; } = null!;
    [Export] public Label DetailName { get; set; } = null!;
    [Export] public Button ActionButton { get; set; } = null!;
    [Export] public Label Hint { get; set; } = null!;
    [Export] public Label Summary { get; set; } = null!;
    [Export] public StyleBoxFlat RowNormal { get; set; } = null!;
    [Export] public StyleBoxFlat RowHover { get; set; } = null!;
    [Export] public StyleBoxFlat RowSelected { get; set; } = null!;

    public event Action? Opened;

    private sealed record ViewItem(string Key, string DefinitionId, int Quantity,
        string? InstanceId = null, EquipmentSlot? WornSlot = null);
    private readonly List<(Button Button, string Category, Action Handler)> _filters = [];
    private readonly List<(Button Button, ViewItem Item)> _itemButtons = [];
    private readonly List<(EquipmentSlot Slot, Button Button, TextureRect Layer, Action Handler)> _slots = [];
    private readonly Dictionary<string, Texture2D> _icons = new();
    private ICampaignSession? _session;
    private IPartyInventory? _inventory;
    private IItemCatalog? _catalog;
    private InventoryState? _displayed;
    private IDisposable? _pauseHold;
    private Button? _toggleButton;
    private ViewItem? _selected;
    private string _category = "All";

    public void Initialize(ICampaignSession session, IPartyInventory inventory, IItemCatalog catalog,
        string characterName, string houseName, Button toggleButton)
    {
        _session = session;
        _inventory = inventory;
        _catalog = catalog;
        CharacterName.Text = characterName;
        HouseName.Text = houseName;
        _toggleButton = toggleButton;
        _toggleButton.Pressed += Open;
        Refresh();
    }

    public override void _Ready()
    {
        CloseButton.Pressed += Close;
        ActionButton.Pressed += ChangeEquipment;
        foreach (var button in CategoryBar.GetChildren().OfType<Button>())
        {
            var category = button.GetMeta("category").AsString();
            Action handler = () => { _category = category; _selected = null; Refresh(); };
            _filters.Add((button, category, handler));
            button.Pressed += handler;
        }
        foreach (var slot in Enum.GetValues<EquipmentSlot>())
        {
            var nodeName = slot switch { EquipmentSlot.MainHand => "Mainhand", EquipmentSlot.OffHand => "Offhand", _ => slot.ToString() };
            var button = CharacterStage.GetNode<Button>(nodeName);
            var layer = CharacterStage.GetNode<TextureRect>($"{nodeName}Layer");
            Action handler = () => SelectSlot(slot);
            button.Pressed += handler;
            _slots.Add((slot, button, layer, handler));
        }
    }

    public override void _Process(double delta)
    {
        if (Visible && _inventory is not null && !ReferenceEquals(_displayed, _inventory.State)) Refresh();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("campaign_inventory", exactMatch: true))
        {
            if (Visible) Close(); else Open();
            GetViewport().SetInputAsHandled();
        }
        else if (Visible && @event is InputEventKey { Pressed: true, Echo: false, Keycode: Key.Escape })
        {
            Close();
            GetViewport().SetInputAsHandled();
        }
    }

    public override void _ExitTree()
    {
        CloseButton.Pressed -= Close;
        ActionButton.Pressed -= ChangeEquipment;
        if (_toggleButton is not null) _toggleButton.Pressed -= Open;
        foreach (var filter in _filters) filter.Button.Pressed -= filter.Handler;
        foreach (var slot in _slots) slot.Button.Pressed -= slot.Handler;
        _pauseHold?.Dispose();
        _pauseHold = null;
    }

    private void Open()
    {
        if (Visible || _session is null || _session.IsPauseLocked) return;
        _pauseHold = _session.HoldPause();
        Refresh();
        Visible = true;
        Opened?.Invoke();
        _filters.First(filter => filter.Category == _category).Button.GrabFocus();
    }

    private void Close()
    {
        Visible = false;
        _pauseHold?.Dispose();
        _pauseHold = null;
        if (_toggleButton is not null && IsInstanceValid(_toggleButton) && _toggleButton.IsInsideTree())
            _toggleButton.GrabFocus();
    }

    private Texture2D Icon(string name)
    {
        if (!_icons.TryGetValue(name, out var icon))
        {
            icon = GD.Load<Texture2D>($"res://assets/ui/inventory/{name}.svg");
            _icons.Add(name, icon);
        }
        return icon;
    }

    private static string IconName(string definitionId) => definitionId == "coat" ? "armor" : definitionId;
    private static string SlotName(EquipmentSlot slot) => slot switch
    {
        EquipmentSlot.MainHand => "Main hand", EquipmentSlot.OffHand => "Off hand", _ => slot.ToString()
    };
    private static string SlotIcon(EquipmentSlot slot) => slot switch
    {
        EquipmentSlot.Head => "helmet", EquipmentSlot.Body => "armor", EquipmentSlot.Hands => "gloves",
        EquipmentSlot.Feet => "boots", EquipmentSlot.MainHand => "sword", _ => "shield"
    };

    private void Refresh()
    {
        if (_inventory is null || _catalog is null) return;
        var focusedItem = _itemButtons.FirstOrDefault(entry => entry.Button.HasFocus()).Item?.Key;
        var state = _inventory.State;
        var carried = state.Items.Select(item => new ViewItem(item.InstanceId, item.DefinitionId, 1, item.InstanceId))
            .Concat(state.Stacks.Select(stack => new ViewItem($"stack:{stack.DefinitionId}", stack.DefinitionId, stack.Quantity)))
            .ToArray();
        var items = carried.Where(item => _category == "All" || _catalog.GetDefinition(item.DefinitionId).Category.ToString() == _category)
            .OrderBy(item => _catalog.GetDefinition(item.DefinitionId).Category)
            .ThenBy(item => _catalog.GetDefinition(item.DefinitionId).Name, StringComparer.Ordinal)
            .ThenBy(item => item.Key, StringComparer.Ordinal).ToArray();
        var worn = state.Equipment.Select(entry => new ViewItem(entry.Item.InstanceId, entry.Item.DefinitionId, 1,
            entry.Item.InstanceId, entry.Slot)).ToArray();
        var selection = carried.Concat(worn).FirstOrDefault(item => item.Key == _selected?.Key);
        foreach (var filter in _filters) filter.Button.SetPressedNoSignal(filter.Category == _category);
        foreach (var child in ItemList.GetChildren())
        {
            ItemList.RemoveChild(child);
            child.QueueFree();
        }
        _itemButtons.Clear();
        CategoryHeading.Text = _category == "All" ? "ALL ITEMS" : _category.ToUpperInvariant();
        ItemCount.Text = $"{items.Length} {(items.Length == 1 ? "entry" : "entries")}";
        Summary.Text = $"{state.Items.Count} carried · {state.Equipment.Count} equipped · {state.Stacks.Count} stacks";
        foreach (var item in items)
        {
            var definition = _catalog.GetDefinition(item.DefinitionId);
            var button = new InventoryItemButton
            {
                Name = item.Key.Replace(":", "-"),
                CustomMinimumSize = new Vector2(0, 40),
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                MouseDefaultCursorShape = CursorShape.PointingHand,
                TooltipText = ItemTooltip(item),
                ItemIcon = Icon(IconName(item.DefinitionId)),
                ItemTitle = definition.Name
            };
            button.AddThemeStyleboxOverride("normal", RowNormal);
            button.AddThemeStyleboxOverride("hover", RowHover);
            button.AddThemeStyleboxOverride("pressed", RowSelected);
            button.AddThemeStyleboxOverride("focus", RowSelected);
            button.Pressed += () => SelectItem(item);
            ItemList.AddChild(button);
            var margin = new MarginContainer { Name = "Margin", MouseFilter = MouseFilterEnum.Ignore };
            button.AddChild(margin);
            margin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            margin.AddThemeConstantOverride("margin_left", 10);
            margin.AddThemeConstantOverride("margin_right", 12);
            var row = new HBoxContainer { Name = "Row", MouseFilter = MouseFilterEnum.Ignore };
            row.AddThemeConstantOverride("separation", 10);
            margin.AddChild(row);
            row.AddChild(new TextureRect
            {
                Texture = button.ItemIcon, CustomMinimumSize = new Vector2(28, 28),
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                SizeFlagsVertical = SizeFlags.ShrinkCenter, MouseFilter = MouseFilterEnum.Ignore
            });
            row.AddChild(new Label
            {
                Text = definition.Name, SizeFlagsHorizontal = SizeFlags.ExpandFill,
                TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis, MouseFilter = MouseFilterEnum.Ignore
            });
            row.AddChild(new Label
            {
                Name = "Quantity", Text = item.Quantity.ToString(System.Globalization.CultureInfo.InvariantCulture),
                CustomMinimumSize = new Vector2(42, 0), HorizontalAlignment = HorizontalAlignment.Right,
                MouseFilter = MouseFilterEnum.Ignore
            });
            _itemButtons.Add((button, item));
        }
        if (items.Length == 0)
        {
            var empty = new Label { Text = "No items in this category.", HorizontalAlignment = HorizontalAlignment.Center };
            empty.AddThemeColorOverride("font_color", new Color("919b80"));
            ItemList.AddChild(empty);
        }
        foreach (var slot in _slots)
        {
            var equipped = state.Equipment.FirstOrDefault(entry => entry.Slot == slot.Slot);
            slot.Button.Icon = Icon(equipped is null ? SlotIcon(slot.Slot) : IconName(equipped.Item.DefinitionId));
            slot.Button.Modulate = equipped is null ? new Color(0.7f, 0.72f, 0.65f, 0.65f) : Colors.White;
            slot.Button.TooltipText = equipped is null ? $"{SlotName(slot.Slot)} · Empty"
                : ItemTooltip(new ViewItem(equipped.Item.InstanceId, equipped.Item.DefinitionId, 1, equipped.Item.InstanceId, slot.Slot));
            slot.Layer.Visible = equipped is not null;
            if (equipped is not null) slot.Layer.Texture = Icon($"worn-{equipped.Item.DefinitionId}");
        }
        _displayed = state;
        SelectItem(selection ?? items.FirstOrDefault());
        if (Visible && focusedItem is not null)
        {
            var replacement = _itemButtons.FirstOrDefault(entry => entry.Item.Key == focusedItem).Button;
            (replacement ?? _filters.First(filter => filter.Category == _category).Button).GrabFocus();
        }
    }

    private string ItemTooltip(ViewItem item)
    {
        var definition = _catalog!.GetDefinition(item.DefinitionId);
        var location = item.WornSlot is { } worn ? $"Equipped · {SlotName(worn)}"
            : item.InstanceId is not null ? "Individual item · Quantity 1" : $"Stack · Quantity {item.Quantity}";
        var slot = definition.Slot is { } compatible ? $"\nSlot: {SlotName(compatible)}" : "";
        return $"{definition.Name}\n{definition.Category} · {location}{slot}\n\n{definition.Description}";
    }

    private void SelectSlot(EquipmentSlot slot)
    {
        var equipped = _inventory?.State.Equipment.FirstOrDefault(entry => entry.Slot == slot);
        SelectItem(equipped is null ? null : new ViewItem(equipped.Item.InstanceId, equipped.Item.DefinitionId,
            1, equipped.Item.InstanceId, slot));
        if (equipped is null)
        {
            DetailName.Text = $"{SlotName(slot)} · Empty";
            Hint.Text = "Select a compatible item from the inventory to equip it.";
        }
    }

    private void SelectItem(ViewItem? item)
    {
        _selected = item;
        var definition = item is null ? null : _catalog!.GetDefinition(item.DefinitionId);
        DetailName.Text = definition?.Name ?? "No item selected";
        ActionButton.Visible = definition?.Slot is not null;
        ActionButton.Text = item?.WornSlot is not null ? "Unequip" : "Equip";
        ActionButton.TooltipText = definition?.Slot is not null ? $"Equipment slot: {SlotName(definition.Slot.Value)}" : "";
        Hint.Text = definition?.Slot is null ? "Hover for details. Select an item or equipment slot to inspect it."
            : item?.WornSlot is not null ? "Unequip returns this item to the party inventory."
            : "Equip replaces the slot's item and returns it to the inventory.";
        foreach (var entry in _itemButtons)
            entry.Button.AddThemeStyleboxOverride("normal", entry.Item.Key == item?.Key ? RowSelected : RowNormal);
        foreach (var slot in _slots)
            slot.Button.AddThemeStyleboxOverride("normal", item?.WornSlot == slot.Slot ? RowSelected : RowNormal);
        ConfigureFocus();
    }

    private void ChangeEquipment()
    {
        if (_selected?.InstanceId is null || _inventory is null) return;
        var result = _selected.WornSlot is { } slot ? _inventory.Unequip(slot) : _inventory.Equip(_selected.InstanceId);
        Refresh();
        if (result != InventoryResult.Success) Hint.Text = "Equipment changed. Select the item again.";
        if (ActionButton.Visible) ActionButton.GrabFocus();
    }

    private void ConfigureFocus()
    {
        var controls = _filters.Select(filter => filter.Button).Concat(_slots.Select(slot => slot.Button))
            .Concat(_itemButtons.Select(entry => entry.Button))
            .Concat(ActionButton.Visible ? new[] { ActionButton } : Array.Empty<Button>()).Append(CloseButton).ToArray();
        for (var index = 0; index < controls.Length; index++)
        {
            var button = controls[index];
            var previous = button.GetPathTo(controls[(index + controls.Length - 1) % controls.Length]);
            var next = button.GetPathTo(controls[(index + 1) % controls.Length]);
            button.FocusPrevious = previous;
            button.FocusNext = next;
            button.FocusNeighborLeft = previous;
            button.FocusNeighborTop = previous;
            button.FocusNeighborRight = next;
            button.FocusNeighborBottom = next;
        }
    }
}
