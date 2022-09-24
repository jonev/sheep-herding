using System.Numerics;
using SheepHerding.Api.Entities;

namespace SheepHerding.Api.Helpers;

public static class Calculator
{
    public static double DegreesToRadians(double angle)
    {
        return (Math.PI / 180) * angle;
    }
    
    public static double RadiansToDegrees(double angle)
    {
        return (180/Math.PI) * angle;
    }

    public static double AngleInDegrees(double x1, double y1, double x2, double y2)
    {
        return Math.Atan2(y2 - y1, x2 - x1) * 180.0 / Math.PI;
    }
    
    public static double AngleInDegrees(Vector2 a, Vector2 b)
    {
        return Math.Atan2(b.Y - a.Y, b.X - a.X) * 180.0 / Math.PI;
    }
    
    public static double AngleInRadiansLimited(Vector2 a, Vector2 b)
    {
        var result = Math.Atan2(b.Y, b.X) - Math.Atan2(a.Y, a.X);
        while (result < -Math.PI) result += 2*Math.PI;
        while (result > Math.PI) result -= 2*Math.PI;
        return result;
    }

    public static double AngleInRadians(Vector2 a, Vector2 b)
    {
        return Math.Atan2(b.Y, b.X) - Math.Atan2(a.Y, a.X);
    }

    public static double Length(double x1, double y1, double x2, double y2)
    {
        return Math.Sqrt((Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2)));
    }

    public static Coordinate Centroid(IList<Coordinate> coordinates)
    {
        var sumX = coordinates.Select(c => c.X).Sum();
        var sumY = coordinates.Select(c => c.Y).Sum();
        return new Coordinate(sumX / coordinates.Count, sumY / coordinates.Count);
    }

    public static Vector2 FlipLength(Vector2 vector, double maxValue)
    {
        var norm = Vector2.Normalize(vector);
        var length = maxValue - vector.Length();
        if (length < 0.0) return Vector2.Zero;
        return Vector2.Multiply(norm, (float) length);
    }
    
    public static Vector2 FlipExLength(Vector2 vector, double maxValue)
    {
        var norm = Vector2.Normalize(vector);
        var length = ExponentialDecrease((vector.Length()/maxValue), 10.0);
        if (length < 0.0) return Vector2.Zero;
        return Vector2.Multiply(norm, (float) (length * maxValue));
    }

    public static double ExponentialDecrease(double x, double curve)
    {
        return -1 * ((Math.Pow(curve, x) - 1) / (curve - 1)) + 1;
    }
    
    public static double ExponentialIncrease(double x, double curve)
    {
        return ((Math.Pow(curve, x) - 1) / (curve - 1));
    }

    public static Vector2 RotateVector(Vector2 vector, double anglesInRadians)
    {
        var newX = vector.X * Math.Cos(anglesInRadians) - vector.Y * Math.Sin(anglesInRadians);
        var newY = vector.X * Math.Sin(anglesInRadians) + vector.Y * Math.Cos(anglesInRadians);
        return new Vector2((float) newX, (float) newY);
    }
    
    public static Vector2 Pull(Vector2 v, double factor, double speed)
    {
        if (factor > 1.00) throw new ArgumentException();
        if (factor < 0.0) throw new ArgumentException();
        
        var r = ExponentialIncrease(factor, 10.0);
        var normalized = Vector2.Normalize(v);
        return Vector2.Multiply(normalized, (float)(r*speed));
    }
    
    public static Vector2 Push(Vector2 v, double factor, double speed)
    {
        if (factor > 1.00) throw new ArgumentException();
        if (factor < 0.0) throw new ArgumentException();
        
        var r = ExponentialDecrease(factor, 10.0);
        var normalized = Vector2.Normalize(v);
        return Vector2.Multiply(normalized, (float)(r*speed));
    }
    
    public static bool InRange(Coordinate a, Coordinate b, double under, double over)
    {
        var v = Converter.ToVector2(a, b);
        return Math.Abs(v.Length()) < under && Math.Abs(v.Length()) > over;
    }
    
    public static bool UnderWithHysteresis(bool value, Coordinate a, Coordinate b, double under, double hysteresis)
    {
        var lenght = Math.Abs(Converter.ToVector2(a, b).Length());
        if (lenght < under)
        {
            return true;
        }

        if (lenght > under + hysteresis)
        {
            return false;
        }

        return value;
    }
    
    public static Vector2 GetCommandVector(Coordinate position, Coordinate current, Coordinate next)
    {
        var positionCurrentVector = Converter.ToVector2(position, current);
        var currentNextVector = Converter.ToVector2(current, next);
        
        var angleInRadians = AngleInRadians(positionCurrentVector, currentNextVector);

        if (angleInRadians < Math.PI/4) // If the andle is under 90 degrees we do now want any adjustments
        {
            return Vector2.Zero;
        }
        
        var rotatedNormNextVector = Vector2.Normalize(RotateVector(currentNextVector,  
            angleInRadians)); 
        
        return rotatedNormNextVector;
    }

    public static double IncreaseWhenUnder(double master, double threshold, double valueToIncrease)
    {
        if (master > threshold) return valueToIncrease;

        return valueToIncrease + Math.PI/4;
    }
}