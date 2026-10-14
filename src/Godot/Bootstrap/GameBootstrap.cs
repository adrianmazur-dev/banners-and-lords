using Godot;

namespace Game.Godot.Bootstrap;

public partial class GameBootstrap : Node
{
    public override void _Ready()
    {
        GD.Print("Banners & Lords initialized.");
    }
}
