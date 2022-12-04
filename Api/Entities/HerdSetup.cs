namespace SheepHerding.Api.Entities;

public class HerdSetup
{
    public HerdSetup(PathCoordinator predefinedPathCoordinator, List<Coordinate> sheepStartCoordinates,
        PathCoordinator terrainPath)
    {
        PredefinedPathCoordinator = predefinedPathCoordinator;
        SheepStartCoordinates = sheepStartCoordinates;
        TerrainPath = terrainPath;
    }

    public PathCoordinator PredefinedPathCoordinator { get; }
    public List<Coordinate> SheepStartCoordinates { get; }
    public PathCoordinator TerrainPath { get; }
}