using System.Numerics;
using SheepHerding.Api.Entities;

namespace SheepHerding.Api.Helpers;

public static class Calculator
{
    public static double AngleToRadians(double angle)
    {
        return (Math.PI / 180) * angle;
    }

    public static double AngleInDegrees(double x1, double y1, double x2, double y2)
    {
        return Math.Atan2(y2 - y1, x2 - x1) * 180.0 / Math.PI;
    }
    
    public static double AngleInRadians(double x1, double y1, double x2, double y2)
    {
        return Math.Atan2(y2 - y1, x2 - x1);
    }
    
    public static double Length(double x1, double y1, double x2, double y2)
    {
        return Math.Sqrt((Math.Pow(x2- x1, 2) + Math.Pow(y2 - y1, 2)));
    }
    
    public static (double x, double y) Centroid(IList<Coordinate> coordinates)
    {
        var sumX = coordinates.Select(c => c.X).Sum();
        var sumY = coordinates.Select(c => c.Y).Sum();
        return (sumX / coordinates.Count, sumY / coordinates.Count);
    }
    
    public static Vector2 FlipLength(Vector2 vector, double maxValue)
    {
        var norm = Vector2.Normalize(vector);
        var length = maxValue - vector.Length();
        if (length < 0.0) return Vector2.Zero;
        return Vector2.Multiply(norm, (float)length);
    }
}