using SheepHerding.Api.Entities;

namespace SheepHerding.Api.Helpers;

public static class PredefinedPaths
{
    public static List<AckableCoordinate> TwoUTurns()
    {
        return new List<AckableCoordinate>()
        {
            new(0, 150, 200),
            new(1, 300, 200),
            new(2, 400, 200),
            new(3, 500, 200),
            new(4, 600, 200),
            new(5, 800, 200),
            new(6, 800, 400),
            new(7, 400, 400),
            new(8, 200, 400),
            new(9, 200, 700),
            new(10, 900, 900)
        };
    }
    
    public static List<AckableCoordinate> SmallTurns()
    {
        return new List<AckableCoordinate>
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
    }
    
    public static List<AckableCoordinate> SmallAnd90DegreesTurn()
    {
        return new List<AckableCoordinate>
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
    }
    
    public static List<AckableCoordinate> s90DegreesLeftTestTurn()
    {
        return new List<AckableCoordinate>
        {
            new(0, 100, 100),
            new(1, 200, 150),
            new(2, 200, 400),
            new(2, 200, 600),
            new(3, 600, 600),
            new(4, 950, 850),
        }; 
    }
}