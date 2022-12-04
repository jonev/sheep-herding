using SheepHerding.Api.Entities;

namespace SheepHerding.Api.Helpers;

public class PredefinedHerdSetup
{
    public HerdSetup GetSetup(int nr)
    {
        switch (nr)
        {
            case 0:
                return FromAToB();
            case 1:
                return PathCrossTesting();
            case 2:
                return SmallTurns();
            case 3:
                return TwoUTurns();
            case 4:
                return s90DegreesLeftTestTurn();
            case 5:
                return SmallAnd90DegreesTurn();
            case 6:
                return FromAToBWithX();
            default:
                return FromAToB();
        }
    }

    private HerdSetup FromAToB()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (int i = 0; i < 5; i++)
        {
            listOfSheepCoordinates.Add(new Coordinate(200 + ((i % 10) * 20), 250 + ((i % 3) * 20)));
        }

        var p = new PathCoordinator
        (
            new PathCoordinate(new Coordinate(350, 350),
                new PathCoordinate(new Coordinate(950, 850), null)));
        return new HerdSetup(p, listOfSheepCoordinates, p);
    }

    private HerdSetup FromAToBWithX()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (int i = 0; i < 5; i++)
        {
            listOfSheepCoordinates.Add(new Coordinate(200 + ((i % 10) * 20), 250 + ((i % 3) * 20)));
        }

        var p = new PathCoordinator
        (
            new PathCoordinate(new Coordinate(350, 350),
                new PathCoordinate(new Coordinate(450, 450),
                    new PathCross(
                        new PathCoordinate(new Coordinate(550, 450),
                            new PathCoordinate(new Coordinate(650, 450),
                                new PathCoordinate(new Coordinate(1000, 450), null))),
                        new PathCoordinate(new Coordinate(550, 550),
                            new PathCoordinate(new Coordinate(750, 750),
                                new PathCoordinate(new Coordinate(950, 850), null)))
                    ))));

        return new HerdSetup(p, listOfSheepCoordinates, p);
    }

    private HerdSetup PathCrossTesting()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (int i = 0; i < 5; i++)
        {
            listOfSheepCoordinates.Add(new Coordinate(200 + ((i % 10) * 20), 250 + ((i % 3) * 20)));
        }

        var p = new PathCoordinator
        (
            new PathCoordinate(new Coordinate(500, 300),
                new PathCoordinate(new Coordinate(800, 300),
                    new PathCoordinate(new Coordinate(950, 850), null))));

        return new HerdSetup(p, listOfSheepCoordinates, p);
    }

    private HerdSetup SmallTurns()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (int i = 0; i < 5; i++)
        {
            listOfSheepCoordinates.Add(new Coordinate(800 + ((i % 10) * 20), 200 + ((i % 3) * 20)));
        }

        var p = new PathCoordinator
        (
            new PathCoordinate(new Coordinate(100, 100),
                new PathCoordinate(new Coordinate(200, 150),
                    new PathCoordinate(new Coordinate(300, 250),
                        new PathCoordinate(new Coordinate(400, 450),
                            new PathCoordinate(new Coordinate(600, 500),
                                new PathCoordinate(new Coordinate(700, 600),
                                    new PathCoordinate(new Coordinate(850, 700),
                                        new PathCoordinate(new Coordinate(950, 850), null)))))))));

        return new HerdSetup(p, listOfSheepCoordinates, p);
    }

    private HerdSetup TwoUTurns()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (int i = 0; i < 5; i++)
        {
            listOfSheepCoordinates.Add(new Coordinate(800 + ((i % 10) * 20), 200 + ((i % 3) * 20)));
        }

        var p = new PathCoordinator
        (
            new PathCoordinate(new Coordinate(150, 200),
                new PathCoordinate(new Coordinate(600, 200),
                    new PathCoordinate(new Coordinate(600, 500),
                        new PathCoordinate(new Coordinate(200, 500),
                            new PathCoordinate(new Coordinate(200, 800),
                                new PathCoordinate(new Coordinate(950, 850), null)))))));
        return new HerdSetup(p, listOfSheepCoordinates, p);
    }

    private HerdSetup s90DegreesLeftTestTurn()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (int i = 0; i < 5; i++)
        {
            listOfSheepCoordinates.Add(new Coordinate(800 + ((i % 10) * 20), 200 + ((i % 3) * 20)));
        }

        var p = new PathCoordinator
        (
            new PathCoordinate(new Coordinate(100, 100),
                new PathCoordinate(new Coordinate(200, 150),
                    new PathCoordinate(new Coordinate(200, 400),
                        new PathCoordinate(new Coordinate(200, 600),
                            new PathCoordinate(new Coordinate(600, 600),
                                new PathCoordinate(new Coordinate(950, 850), null)))))));

        return new HerdSetup(p, listOfSheepCoordinates, p);
    }

    private HerdSetup SmallAnd90DegreesTurn()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (int i = 0; i < 5; i++)
        {
            listOfSheepCoordinates.Add(new Coordinate(800 + ((i % 10) * 20), 200 + ((i % 3) * 20)));
        }

        var p = new PathCoordinator
        (
            new PathCoordinate(new Coordinate(100, 100),
                new PathCoordinate(new Coordinate(200, 150),
                    new PathCoordinate(new Coordinate(300, 250),
                        new PathCoordinate(new Coordinate(400, 450),
                            new PathCoordinate(new Coordinate(600, 500),
                                new PathCoordinate(new Coordinate(600, 700),
                                    new PathCoordinate(new Coordinate(850, 700),
                                        new PathCoordinate(new Coordinate(950, 850), null)))))))));

        return new HerdSetup(p, listOfSheepCoordinates, p);
    }
}