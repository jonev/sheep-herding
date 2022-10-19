using SheepHerding.Api.Entities;

namespace SheepHerding.Api.Helpers;

public static class PredefinedPaths
{
    public static (List<AckableCoordinate> HerdingPath, List<Coordinate> SheepStartCoordinates, List<Coordinate> TerrainPath) TwoUTurns()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (int i = 0; i < 5; i++)
        {
            listOfSheepCoordinates.Add(new Coordinate(800 + ((i % 10) * 20), 200 + ((i % 3) * 20)));
        }

        var path = new List<AckableCoordinate>()
        {
            new(0, 150, 200),
            new(1, 600, 200),
            new(2, 600, 500),
            new(3, 200, 500),
            new(4, 200, 800),
            new(5, 900, 800)
        };
        return (path, listOfSheepCoordinates, path.Select(p => new Coordinate(p.X, p.Y)).ToList());
    }

    public static (List<AckableCoordinate> Path, List<Coordinate> SheepStartCoordinates,  List<Coordinate> TerrainPath) PathCrossTesting()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (int i = 0; i < 5; i++)
        {
            // listOfSheepCoordinates.Add(new Coordinate(100, 300));
            listOfSheepCoordinates.Add(new Coordinate(200 + ((i % 10) * 20), 250 + ((i % 3) * 20)));
            
        }

        var path = new List<AckableCoordinate>()
        {
            new(0, 500, 300),
            new(1, 800, 300),
            new(2, 900, 900),
        };
        
        // var terrainPath = new List<Coordinate>()
        // {
        //     new(50, 300),
        //     new(950, 300),
        // };
        return (path, listOfSheepCoordinates, path.Select(p => new Coordinate(p.X, p.Y)).ToList());
    }

    public static (List<AckableCoordinate> Path, List<Coordinate> SheepStartCoordinates,  List<Coordinate> TerrainPath) SmallTurns()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (int i = 0; i < 5; i++)
        {
            listOfSheepCoordinates.Add(new Coordinate(800 + ((i % 10) * 20), 200 + ((i % 3) * 20)));
        }

        var path = new List<AckableCoordinate>()
        {
            new(0, 100, 100),
            new(1, 200, 150),
            new(2, 300, 250),
            new(3, 400, 450),
            new(4, 600, 500),
            new(5, 700, 600),
            new(6, 850, 700),
            new(7, 950, 850),
        };
        return (path, listOfSheepCoordinates, path.Select(p => new Coordinate(p.X, p.Y)).ToList());
    }

    public static (List<AckableCoordinate> Path, List<Coordinate> SheepStartCoordinates,  List<Coordinate> TerrainPath) SmallAnd90DegreesTurn()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (int i = 0; i < 5; i++)
        {
            listOfSheepCoordinates.Add(new Coordinate(800 + ((i % 10) * 20), 200 + ((i % 3) * 20)));
        }

        var path = new List<AckableCoordinate>()
        {
            new(0, 100, 100),
            new(1, 200, 150),
            new(2, 300, 250),
            new(3, 400, 450),
            new(4, 600, 500),
            new(5, 600, 700),
            new(6, 850, 700),
            new(7, 950, 850),
        };
        return (path, listOfSheepCoordinates, path.Select(p => new Coordinate(p.X, p.Y)).ToList());
    }

    public static (List<AckableCoordinate> Path, List<Coordinate> SheepStartCoordinates,  List<Coordinate> TerrainPath) s90DegreesLeftTestTurn()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (int i = 0; i < 5; i++)
        {
            listOfSheepCoordinates.Add(new Coordinate(800 + ((i % 10) * 20), 200 + ((i % 3) * 20)));
        }

        var path = new List<AckableCoordinate>()
        {
            new(0, 100, 100),
            new(1, 200, 150),
            new(2, 200, 400),
            new(2, 200, 600),
            new(3, 600, 600),
            new(4, 950, 850),
        };
        return (path, listOfSheepCoordinates, path.Select(p => new Coordinate(p.X, p.Y)).ToList());
    }
}