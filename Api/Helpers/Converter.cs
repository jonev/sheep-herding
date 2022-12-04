using System.Numerics;
using SheepHerding.Api.Entities;

namespace SheepHerding.Api.Helpers;

public static class Converter
{
    public static Vector2 ToVector2(double x1, double y1, double x2, double y2)
    {
        return new Vector2(Convert.ToSingle(x2 - x1),
            Convert.ToSingle(y2 - y1));
    }
    
    public static Vector2 ToVector2(Coordinate a, Coordinate b)
    {
        if (a is null || b is null)
        {
            throw new ArgumentException("Coordinates cant be null");
        }
        return new Vector2(Convert.ToSingle(b.X - a.X),
            Convert.ToSingle(b.Y - a.Y));
    }
    
    public static Vector2 ToVector2Negated(Coordinate a, Coordinate b)
    {
        return Vector2.Negate(ToVector2(a, b));
    }
    
    public static Vector2 ToVector2Norm(Coordinate a, Coordinate b)
    {
        return Vector2.Normalize(ToVector2(a, b));

    }
    
    public static Vector2 ToVector2NormNegated(Coordinate a, Coordinate b)
    {
        return Vector2.Negate(Vector2.Normalize(ToVector2(a, b)));

    }
}