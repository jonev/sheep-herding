namespace SheepHerding.Api.Entities;

public class HerdSetup
{
    public List<AckableCoordinate> Path { get; }
    public List<Coordinate> SheepStartCoordinates { get; }
    public List<Coordinate> TerrainPath { get; }

    public HerdSetup(List<AckableCoordinate> path, List<Coordinate> sheepStartCoordinates, List<Coordinate> terrainPath)
    {
        Path = path;
        SheepStartCoordinates = sheepStartCoordinates;
        TerrainPath = terrainPath;
    }
}