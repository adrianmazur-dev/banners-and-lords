using System;
using System.Linq;
using Godot;
using Game.Simulation.Features.WorldMap.Contracts;

namespace Game.Godot.Features.WorldMap;

public partial class ValleyTerrain : Node2D
{
    private static readonly Color Ink = new("514e3c");
    private WorldMapDefinition? _map;
    private Vector2[] _road = [];
    private Vector2[] _river = [];

    public void Initialize(WorldMapDefinition map)
    {
        _map = map;
        _road = map.Road.Select(point => new Vector2(point.X, point.Y)).ToArray();
        _river = map.River.Select(point => new Vector2(point.X, point.Y)).ToArray();
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_map is null)
        {
            return;
        }

        var size = new Vector2(_map.Size.X, _map.Size.Y);
        DrawRect(new Rect2(Vector2.Zero, size), new Color("c9c8a5"));
        var random = new Random(27);
        for (var index = 0; index < 180; index++)
        {
            var point = new Vector2(random.Next(60, 1140), random.Next(24, 676));
            Oval(point, new Vector2(random.Next(12, 55), random.Next(4, 14)), new Color(0.39f, 0.46f, 0.29f, 0.07f));
            DrawLine(point, point + new Vector2(3, -4), new Color(0.35f, 0.41f, 0.26f, 0.2f), 1, true);
        }

        Hill(new Vector2(175, 610), 90);
        Hill(new Vector2(360, 585), 62);
        Hill(new Vector2(930, 115), 65);
        for (var index = 0; index < 7; index++)
        {
            Mountain(new Vector2(380 + index * 49, 85 + index % 3 * 16), 44 + index % 3 * 12);
        }

        DrawPolyline(_river, new Color("9aab85"), 54, true);
        DrawPolyline(_river, new Color("697e76"), 39, true);
        DrawPolyline(_river, new Color("88a6a1"), 33, true);
        DrawPolyline(_river.Select(point => point + new Vector2(-7, 0)).ToArray(), new Color(0.77f, 0.86f, 0.78f, 0.55f), 2, true);

        DrawPolyline(_road, new Color("a69a74"), 15, true);
        DrawPolyline(_road, new Color("dcd1aa"), 11, true);
        DrawPolyline(_road, new Color(0.68f, 0.60f, 0.42f, 0.35f), 1, true);
        Bridge(new Vector2(655, 355));

        Forest(new Vector2(205, 195), new Vector2(150, 85), 44, random);
        Forest(new Vector2(1000, 555), new Vector2(150, 90), 48, random);
        Forest(new Vector2(1080, 105), new Vector2(70, 42), 15, random);
        foreach (var landmark in _map.Landmarks)
        {
            Village(new Vector2(landmark.Position.X, landmark.Position.Y), landmark.Name);
        }

        MapLabel(new Vector2(85, 80), "GREENWOOD", 16, new Color("65704c"));
        MapLabel(new Vector2(895, 660), "SOUTHERN WOODS", 16, new Color("65704c"));
        MapLabel(new Vector2(750, 605), "Alder River", 16, new Color("4d716f"));
        MapLabel(new Vector2(425, 445), "OLD NORTH ROAD", 13, new Color("80734f"));
        DrawRect(new Rect2(Vector2.One * 5, size - Vector2.One * 10), new Color("8c8968"), false, 1.5f);
        DrawRect(new Rect2(Vector2.One, size - Vector2.One * 2), Ink, false, 2);
        Compass(new Vector2(1120, 375));
    }

    private void Forest(Vector2 center, Vector2 radius, int count, Random random)
    {
        Oval(center, radius, new Color(0.31f, 0.39f, 0.24f, 0.18f));
        var trees = new Vector2[count];
        for (var index = 0; index < count; index++)
        {
            var angle = random.NextDouble() * Math.Tau;
            var distance = Math.Sqrt(random.NextDouble());
            trees[index] = center + new Vector2((float)Math.Cos(angle) * radius.X, (float)Math.Sin(angle) * radius.Y) * (float)distance;
        }

        foreach (var tree in trees.OrderBy(point => point.Y))
        {
            Tree(tree, random.Next(20, 34));
        }
    }

    private void Tree(Vector2 origin, float height)
    {
        Oval(origin + new Vector2(3, 3), new Vector2(13, 4), new Color(0.22f, 0.27f, 0.18f, 0.23f));
        DrawLine(origin, origin + new Vector2(0, -9), new Color("6a6146"), 3);
        Vector2[] crown = [origin + new Vector2(0, -height), origin + new Vector2(10, -height * 0.4f), origin + new Vector2(6, -height * 0.4f), origin + new Vector2(15, -5), origin + new Vector2(-13, -5), origin + new Vector2(-6, -height * 0.4f), origin + new Vector2(-10, -height * 0.4f)];
        DrawColoredPolygon(crown, new Color("66764e"));
        DrawPolyline(crown.Append(crown[0]).ToArray(), new Color("4b5b3d"), 1.2f, true);
        DrawLine(origin + new Vector2(0, -height + 5), origin + new Vector2(0, -9), new Color("819064"), 1, true);
    }

    private void Village(Vector2 origin, string name)
    {
        Oval(origin + new Vector2(0, 5), new Vector2(66, 23), new Color(0.58f, 0.51f, 0.32f, 0.18f));
        var field = new Rect2(origin + new Vector2(-77, -6), new Vector2(35, 27));
        DrawRect(field, new Color("b3a16d"));
        for (var index = 1; index < 5; index++)
        {
            var x = field.Position.X + index * 7;
            DrawLine(new Vector2(x, field.Position.Y), new Vector2(x, field.End.Y), new Color("8d8855"), 2, true);
        }

        House(origin + new Vector2(-23, -11), new Vector2(24, 20));
        House(origin + new Vector2(16, -17), new Vector2(29, 24));
        House(origin + new Vector2(1, 10), new Vector2(22, 18));
        House(origin + new Vector2(42, 6), new Vector2(19, 17));
        DrawRect(new Rect2(origin + new Vector2(20, -49), new Vector2(10, 19)), new Color("d7cfab"));
        DrawPolyline([origin + new Vector2(18, -49), origin + new Vector2(25, -60), origin + new Vector2(32, -49)], Ink, 2, true);
        var labelSize = ThemeDB.FallbackFont.GetStringSize(name, fontSize: 20);
        var label = new Rect2(origin + new Vector2(-labelSize.X / 2 - 8, 33), new Vector2(labelSize.X + 16, 28));
        DrawRect(label, new Color(0.87f, 0.85f, 0.69f, 0.94f));
        DrawRect(label, new Color("9b9270"), false, 1);
        MapLabel(label.Position + new Vector2(8, 21), name, 20, Ink);
    }

    private void House(Vector2 origin, Vector2 size)
    {
        var walls = new Rect2(origin - size / 2, size);
        DrawRect(walls, new Color("ded4af"));
        DrawRect(walls, Ink, false, 1.5f);
        Vector2[] roof = [walls.Position + new Vector2(-3, 3), origin + new Vector2(0, -size.Y / 2 - 12), walls.Position + new Vector2(size.X + 3, 3)];
        DrawColoredPolygon(roof, new Color("8f7056"));
        DrawPolyline(roof.Append(roof[0]).ToArray(), Ink, 1.5f, true);
        DrawRect(new Rect2(origin + new Vector2(-2, 2), new Vector2(5, size.Y / 2 - 2)), new Color("72654c"));
    }

    private void Bridge(Vector2 center)
    {
        var deck = new Rect2(center - new Vector2(33, 9), new Vector2(66, 18));
        DrawRect(deck, new Color("ae9971"));
        for (var index = -30; index <= 30; index += 6)
        {
            DrawLine(center + new Vector2(index, -9), center + new Vector2(index, 9), new Color("70664d"), 1, true);
        }

        DrawLine(center + new Vector2(-34, -11), center + new Vector2(34, -11), Ink, 3, true);
        DrawLine(center + new Vector2(-34, 11), center + new Vector2(34, 11), Ink, 3, true);
    }

    private void Mountain(Vector2 origin, float height)
    {
        Vector2[] shape = [origin + new Vector2(-height * 0.7f, 0), origin + new Vector2(0, -height), origin + new Vector2(height * 0.7f, 0)];
        DrawColoredPolygon(shape, new Color("a0a18b"));
        DrawColoredPolygon([shape[1], origin + new Vector2(5, -12), shape[2]], new Color("8c9380"));
        DrawPolyline(shape.Append(shape[0]).ToArray(), new Color("737c6b"), 1, true);
        DrawColoredPolygon([origin + new Vector2(-10, -height + 14), shape[1], origin + new Vector2(12, -height + 17), origin + new Vector2(2, -height + 13)], new Color("d6d4b9"));
    }

    private void Hill(Vector2 origin, float radius)
    {
        for (var ring = 0; ring < 3; ring++)
        {
            var points = new Vector2[25];
            for (var index = 0; index < points.Length; index++)
            {
                var angle = index * MathF.Tau / 24;
                points[index] = origin + new Vector2(MathF.Cos(angle) * (radius - ring * 17), MathF.Sin(angle) * (radius - ring * 17) * 0.3f);
            }

            DrawPolyline(points, new Color(0.47f, 0.51f, 0.33f, 0.3f), 1, true);
        }
    }

    private void Compass(Vector2 origin)
    {
        DrawCircle(origin, 23, new Color(0.36f, 0.36f, 0.27f, 0.28f), false, 1, true);
        DrawColoredPolygon([origin + new Vector2(0, -31), origin + new Vector2(7, 7), origin, origin + new Vector2(-7, 7)], new Color("72745a"));
        DrawLine(origin + new Vector2(0, 25), origin, new Color("72745a"), 1, true);
        MapLabel(origin + new Vector2(-6, -39), "N", 16, Ink);
    }

    private void Oval(Vector2 center, Vector2 radius, Color color)
    {
        DrawSetTransform(center, scale: radius);
        DrawCircle(Vector2.Zero, 1, color);
        DrawSetTransform(Vector2.Zero);
    }

    private void MapLabel(Vector2 position, string text, int fontSize, Color color) =>
        DrawString(ThemeDB.FallbackFont, position, text, fontSize: fontSize, modulate: color);
}
