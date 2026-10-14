using System.Numerics;
using Game.Simulation.Features.WorldMap.Contracts;

namespace Game.Simulation.Features.WorldMap.Application;

public static class ValleyScenario
{
    public static Vector2 PlayerStart => new(330, 585);

    public static WorldMapDefinition CreateMap() => new(
        new Vector2(1200, 700),
        Array.AsReadOnly<MapLandmark>([
            new("Fallowfield", new Vector2(250, 465)),
            new("Stoneford", new Vector2(970, 275))
        ]),
        Array.AsReadOnly<Vector2>([
            new(30, 565), new(170, 510), new(250, 465), new(425, 395),
            new(555, 365), new(655, 355), new(790, 330), new(970, 275), new(1180, 225)
        ]),
        Array.AsReadOnly<Vector2>([
            new(730, 0), new(718, 75), new(685, 145), new(700, 235), new(650, 305),
            new(655, 355), new(705, 445), new(675, 535), new(710, 620), new(690, 700)
        ]));
}
