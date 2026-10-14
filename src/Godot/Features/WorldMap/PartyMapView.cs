using Godot;
using Game.Simulation.Features.Campaign.Contracts;
using Game.Simulation.Features.Parties.Contracts;

namespace Game.Godot.Features.WorldMap;

public partial class PartyMapView : Node2D
{
    private IPartyTravel? _party;
    private ICampaignSession? _session;
    private Vector2? _target;

    public bool IsTraveling => _target.HasValue;

    public void Initialize(IPartyTravel party, ICampaignSession session)
    {
        _party = party;
        _session = session;
        Refresh();
    }

    public override void _Process(double delta) => Refresh();

    private void Refresh()
    {
        if (_party is null || _session is null)
        {
            return;
        }

        var status = _party.Status;
        var previous = new Vector2(status.PreviousPosition.X, status.PreviousPosition.Y);
        var current = new Vector2(status.Position.X, status.Position.Y);
        var position = previous.Lerp(current, (float)_session.TickProgress);
        Vector2? target = status.Destination is { } destination
            ? new Vector2(destination.X, destination.Y)
            : position.DistanceSquaredTo(current) > 0.001f ? current : null;

        if (position != Position || target != _target)
        {
            Position = position;
            _target = target;
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        if (_party is null)
        {
            return;
        }

        var gold = new Color("e3c277");
        if (_target is { } worldTarget)
        {
            var target = worldTarget - Position;
            var length = target.Length();
            var direction = target.Normalized();
            for (var distance = 22f; distance < length - 14; distance += 16)
            {
                DrawLine(direction * distance, direction * Mathf.Min(distance + 7, length - 14), new Color("706349"), 2, true);
            }

            DrawCircle(target, 12, new Color("7d523d"), false, 2, true);
            DrawLine(target + new Vector2(-17, 0), target + new Vector2(17, 0), new Color("7d523d"), 1.5f, true);
            DrawLine(target + new Vector2(0, -17), target + new Vector2(0, 17), new Color("7d523d"), 1.5f, true);
        }

        DrawCircle(new Vector2(3, 6), 16, new Color(0.22f, 0.23f, 0.17f, 0.3f));
        DrawCircle(Vector2.Zero, 20, new Color("4e5140"));
        DrawCircle(Vector2.Zero, 18, gold, false, 2, true);
        DrawCircle(new Vector2(0, -3), 10, new Color("ded3b0"));
        DrawCircle(new Vector2(0, -13), 6, new Color("6c6654"));
        DrawLine(new Vector2(8, 3), new Vector2(8, -56), new Color("494b3c"), 3, true);
        Vector2[] banner = [new(9, -54), new(39, -49), new(32, -35), new(9, -38)];
        DrawColoredPolygon(banner, new Color("944b42"));
        DrawPolyline([banner[0], banner[1], banner[2], banner[3], banner[0]], new Color("5c4237"), 1.5f, true);
        DrawLine(new Vector2(20, -49), new Vector2(20, -40), gold, 3, true);
    }
}
