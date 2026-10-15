using Godot;

namespace Game.Godot.Features.Inventory;

public partial class InventoryItemButton : Button
{
    public Texture2D ItemIcon { get; set; } = null!;
    public string ItemTitle { get; set; } = "";

    public override GodotObject _MakeCustomTooltip(string forText)
    {
        var panel = new PanelContainer { MouseFilter = MouseFilterEnum.Ignore };
        panel.AddThemeStyleboxOverride("panel", GetThemeStylebox("panel", "TooltipPanel"));
        var body = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        body.AddThemeConstantOverride("separation", 10);
        panel.AddChild(body);
        var header = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        header.AddThemeConstantOverride("separation", 10);
        body.AddChild(header);
        header.AddChild(new TextureRect
        {
            Texture = ItemIcon, CustomMinimumSize = new Vector2(40, 40),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = MouseFilterEnum.Ignore
        });
        var title = new Label { Text = ItemTitle, MouseFilter = MouseFilterEnum.Ignore };
        title.AddThemeFontSizeOverride("font_size", 18);
        title.AddThemeColorOverride("font_color", new Color("dfc68a"));
        header.AddChild(title);
        var text = new Label
        {
            Text = forText[(forText.IndexOf('\n') + 1)..],
            CustomMinimumSize = new Vector2(280, 0), AutowrapMode = TextServer.AutowrapMode.WordSmart,
            MouseFilter = MouseFilterEnum.Ignore
        };
        text.AddThemeFontSizeOverride("font_size", 14);
        body.AddChild(text);
        return panel;
    }
}
