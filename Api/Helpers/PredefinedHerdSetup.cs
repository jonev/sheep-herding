using SheepHerding.Api.Entities;

namespace SheepHerding.Api.Helpers;

public class PredefinedHerdSetup
{
    private readonly double _intersectionApproachingThreshold = 80.0;

    public HerdSetup GetSetup(int nr)
    {
        switch (nr)
        {
            // Herding
            case 0:
                return HerdingFromAToB();
            case 1:
                return HerdingSimpleRightTurn();
            case 2:
                return HerdingSimpleLeftTurn();
            case 3:
                return HerdingHardRightTurn();
            case 4:
                return HerdingHardLeftTurn();
            case 5:
                return HerdingTwoUTurns();
            case 6:
                return HerdingMultipleDifferentTurns();
            // Fetch herd to path
            case 7:
                return FetchHerdToPath0DegreeRightPath();
            // case 7:
            //     return FetchHerdToPath45DegreeLeftPath();
            case 8:
                return FetchHerdToPath45DegreeRightPath();
            // case 9:
            //     return FetchHerdToPath90DegreeLeftPath();
            case 9:
                return FetchHerdToPath90DegreeRightPath();
            // case 11:
            //     return FetchHerdToPath180DegreeLeftPath(); // Exception
            case 10:
                return FetchHerdToPath180DegreeRightPath();
            // Cross
            case 11:
                return FromAToBWithCross();
            // Guard
            case 12:
                return HerdingFromAToBWithGuard();
            default:
                return HerdingFromAToB();
        }
    }

    #region FetchHerdToPathPaths

    private HerdSetup FetchHerdToPath0DegreeRightPath()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (var i = 0; i < 5; i++) listOfSheepCoordinates.Add(new Coordinate(475 + i % 10 * 20, 450 + i % 3 * 20));

        var p = new PathCoordinator
        (_intersectionApproachingThreshold,
            new PathCoordinate(new Coordinate(100, 100),
                new PathCoordinate(new Coordinate(200, 150),
                    new PathCoordinate(new Coordinate(200, 400),
                        new PathCoordinate(new Coordinate(200, 600),
                            new PathCoordinate(new Coordinate(600, 600),
                                new PathCoordinate(new Coordinate(950, 850), null)))))));

        return new HerdSetup(p, listOfSheepCoordinates, p);
    }

    private HerdSetup FetchHerdToPath45DegreeRightPath()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (var i = 0; i < 5; i++) listOfSheepCoordinates.Add(new Coordinate(550 + i % 10 * 20, 300 + i % 3 * 20));

        var p = new PathCoordinator
        (_intersectionApproachingThreshold,
            new PathCoordinate(new Coordinate(100, 100),
                new PathCoordinate(new Coordinate(200, 150),
                    new PathCoordinate(new Coordinate(200, 400),
                        new PathCoordinate(new Coordinate(200, 600),
                            new PathCoordinate(new Coordinate(600, 600),
                                new PathCoordinate(new Coordinate(950, 850), null)))))));

        return new HerdSetup(p, listOfSheepCoordinates, p);
    }

    private HerdSetup FetchHerdToPath90DegreeRightPath()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (var i = 0; i < 5; i++) listOfSheepCoordinates.Add(new Coordinate(600 + i % 10 * 20, 150 + i % 3 * 20));

        var p = new PathCoordinator
        (_intersectionApproachingThreshold,
            new PathCoordinate(new Coordinate(100, 100),
                new PathCoordinate(new Coordinate(200, 150),
                    new PathCoordinate(new Coordinate(200, 400),
                        new PathCoordinate(new Coordinate(200, 600),
                            new PathCoordinate(new Coordinate(600, 600),
                                new PathCoordinate(new Coordinate(950, 850), null)))))));

        return new HerdSetup(p, listOfSheepCoordinates, p);
    }

    private HerdSetup FetchHerdToPath180DegreeRightPath()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (var i = 0; i < 5; i++) listOfSheepCoordinates.Add(new Coordinate(600 + i % 10 * 20, 150 + i % 3 * 20));

        var p = new PathCoordinator
        (_intersectionApproachingThreshold,
            new PathCoordinate(new Coordinate(100, 100),
                new PathCoordinate(new Coordinate(200, 150),
                    new PathCoordinate(new Coordinate(200, 400),
                        new PathCoordinate(new Coordinate(200, 800),
                            new PathCoordinate(new Coordinate(950, 850), null))))));

        return new HerdSetup(p, listOfSheepCoordinates, p);
    }

    private HerdSetup FetchHerdToPath45DegreeLeftPath()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (var i = 0; i < 5; i++) listOfSheepCoordinates.Add(new Coordinate(300 + i % 10 * 20, 550 + i % 3 * 20));

        var p = new PathCoordinator
        (_intersectionApproachingThreshold,
            new PathCoordinate(new Coordinate(100, 100),
                new PathCoordinate(new Coordinate(200, 150),
                    new PathCoordinate(new Coordinate(400, 200),
                        new PathCoordinate(new Coordinate(600, 200),
                            new PathCoordinate(new Coordinate(600, 600),
                                new PathCoordinate(new Coordinate(950, 850), null)))))));

        return new HerdSetup(p, listOfSheepCoordinates, p);
    }

    private HerdSetup FetchHerdToPath90DegreeLeftPath()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (var i = 0; i < 5; i++) listOfSheepCoordinates.Add(new Coordinate(150 + i % 10 * 20, 550 + i % 3 * 20));

        var p = new PathCoordinator
        (_intersectionApproachingThreshold,
            new PathCoordinate(new Coordinate(100, 100),
                new PathCoordinate(new Coordinate(200, 150),
                    new PathCoordinate(new Coordinate(400, 200),
                        new PathCoordinate(new Coordinate(600, 200),
                            new PathCoordinate(new Coordinate(600, 600),
                                new PathCoordinate(new Coordinate(950, 850), null)))))));

        return new HerdSetup(p, listOfSheepCoordinates, p);
    }

    private HerdSetup FetchHerdToPath180DegreeLeftPath()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (var i = 0; i < 5; i++) listOfSheepCoordinates.Add(new Coordinate(150 + i % 10 * 20, 600 + i % 3 * 20));

        var p = new PathCoordinator
        (_intersectionApproachingThreshold,
            new PathCoordinate(new Coordinate(100, 100),
                new PathCoordinate(new Coordinate(200, 150),
                    new PathCoordinate(new Coordinate(400, 150),
                        new PathCoordinate(new Coordinate(800, 150),
                            new PathCoordinate(new Coordinate(950, 850), null))))));

        return new HerdSetup(p, listOfSheepCoordinates, p);
    }

    #endregion

    #region HerdingPaths

    private HerdSetup HerdingFromAToB()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (var i = 0; i < 5; i++) listOfSheepCoordinates.Add(new Coordinate(200 + i % 10 * 20, 250 + i % 3 * 20));

        var p = new PathCoordinator
        (_intersectionApproachingThreshold,
            new PathCoordinate(new Coordinate(350, 350),
                new PathCoordinate(new Coordinate(950, 850), null)));
        return new HerdSetup(p, listOfSheepCoordinates, p);
    }

    private HerdSetup HerdingSimpleRightTurn()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (var i = 0; i < 5; i++) listOfSheepCoordinates.Add(new Coordinate(200 + i % 10 * 20, 250 + i % 3 * 20));

        var p = new PathCoordinator
        (_intersectionApproachingThreshold,
            new PathCoordinate(new Coordinate(350, 350),
                new PathCoordinate(new Coordinate(700, 450),
                    new PathCoordinate(new Coordinate(950, 850), null))));

        return new HerdSetup(p, listOfSheepCoordinates, p);
    }

    private HerdSetup HerdingSimpleLeftTurn()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (var i = 0; i < 5; i++) listOfSheepCoordinates.Add(new Coordinate(200 + i % 10 * 20, 250 + i % 3 * 20));

        var p = new PathCoordinator
        (_intersectionApproachingThreshold,
            new PathCoordinate(new Coordinate(350, 350),
                new PathCoordinate(new Coordinate(500, 650),
                    new PathCoordinate(new Coordinate(950, 850), null))));

        return new HerdSetup(p, listOfSheepCoordinates, p);
    }

    private HerdSetup HerdingHardRightTurn()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (var i = 0; i < 5; i++) listOfSheepCoordinates.Add(new Coordinate(200 + i % 10 * 20, 250 + i % 3 * 20));

        var p = new PathCoordinator
        (_intersectionApproachingThreshold,
            new PathCoordinate(new Coordinate(350, 350),
                new PathCoordinate(new Coordinate(900, 350),
                    new PathCoordinate(new Coordinate(950, 850), null))));

        return new HerdSetup(p, listOfSheepCoordinates, p);
    }

    private HerdSetup HerdingHardLeftTurn()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (var i = 0; i < 5; i++) listOfSheepCoordinates.Add(new Coordinate(200 + i % 10 * 20, 250 + i % 3 * 20));

        var p = new PathCoordinator
        (_intersectionApproachingThreshold,
            new PathCoordinate(new Coordinate(350, 350),
                new PathCoordinate(new Coordinate(350, 800),
                    new PathCoordinate(new Coordinate(950, 850), null))));

        return new HerdSetup(p, listOfSheepCoordinates, p);
    }

    private HerdSetup HerdingTwoUTurns()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (var i = 0; i < 5; i++) listOfSheepCoordinates.Add(new Coordinate(250 + i % 10 * 20, 200 + i % 3 * 20));

        var p = new PathCoordinator
        (_intersectionApproachingThreshold,
            new PathCoordinate(new Coordinate(350, 350),
                new PathCoordinate(new Coordinate(900, 350),
                    new PathCoordinate(new Coordinate(900, 550),
                        new PathCoordinate(new Coordinate(200, 550),
                            new PathCoordinate(new Coordinate(200, 800),
                                new PathCoordinate(new Coordinate(950, 850), null)))))));
        return new HerdSetup(p, listOfSheepCoordinates, p);
    }

    private HerdSetup HerdingMultipleDifferentTurns()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (var i = 0; i < 5; i++) listOfSheepCoordinates.Add(new Coordinate(250 + i % 10 * 20, 200 + i % 3 * 20));

        var p = new PathCoordinator
        (_intersectionApproachingThreshold,
            new PathCoordinate(new Coordinate(350, 250),
                new PathCoordinate(new Coordinate(900, 450),
                    new PathCoordinate(new Coordinate(900, 650),
                        new PathCoordinate(new Coordinate(600, 650),
                            new PathCoordinate(new Coordinate(200, 550),
                                new PathCoordinate(new Coordinate(200, 800),
                                    new PathCoordinate(new Coordinate(950, 850), null))))))));
        return new HerdSetup(p, listOfSheepCoordinates, p);
    }

    #endregion

    #region SpecialPaths

    private HerdSetup FromAToBWithCross()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (var i = 0; i < 5; i++) listOfSheepCoordinates.Add(new Coordinate(200 + i % 10 * 20, 250 + i % 3 * 20));

        var p = new PathCoordinator
        (_intersectionApproachingThreshold,
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

    private HerdSetup HerdingFromAToBWithGuard()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (var i = 0; i < 5; i++) listOfSheepCoordinates.Add(new Coordinate(200 + i % 10 * 20, 250 + i % 3 * 20));

        var p = new PathCoordinator
        (_intersectionApproachingThreshold,
            new PathCoordinate(new Coordinate(350, 350),
                new PathCoordinate(new Coordinate(950, 850), null)));
        return new HerdSetup(p, listOfSheepCoordinates, p);
    }

    #endregion

    #region Old

    private HerdSetup FetchHerdAndFindClosestPointOfPath()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (var i = 0; i < 5; i++) listOfSheepCoordinates.Add(new Coordinate(800 + i % 10 * 20, 200 + i % 3 * 20));

        var p = new PathCoordinator
        (_intersectionApproachingThreshold,
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


    private HerdSetup s90DegreesLeftTestTurn()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (var i = 0; i < 5; i++) listOfSheepCoordinates.Add(new Coordinate(800 + i % 10 * 20, 200 + i % 3 * 20));

        var p = new PathCoordinator
        (_intersectionApproachingThreshold,
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
        for (var i = 0; i < 5; i++) listOfSheepCoordinates.Add(new Coordinate(800 + i % 10 * 20, 200 + i % 3 * 20));

        var p = new PathCoordinator
        (_intersectionApproachingThreshold,
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

    private HerdSetup SimpleRightTurn1()
    {
        var listOfSheepCoordinates = new List<Coordinate>();
        for (var i = 0; i < 5; i++) listOfSheepCoordinates.Add(new Coordinate(200 + i % 10 * 20, 250 + i % 3 * 20));

        var p = new PathCoordinator
        (_intersectionApproachingThreshold,
            new PathCoordinate(new Coordinate(500, 300),
                new PathCoordinate(new Coordinate(800, 300),
                    new PathCoordinate(new Coordinate(950, 850), null))));

        return new HerdSetup(p, listOfSheepCoordinates, p);
    }

    #endregion
}