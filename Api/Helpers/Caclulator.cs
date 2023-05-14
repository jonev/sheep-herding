using System.Numerics;
using SheepHerding.Api.Entities;

namespace SheepHerding.Api.Helpers;

public static class Calculator
{
    public static double AngleInRadiansLimited(Vector2 a, Vector2 b)
    {
        var result = Math.Atan2(b.Y, b.X) - Math.Atan2(a.Y, a.X);
        while (result < -Math.PI) result += 2 * Math.PI;
        while (result > Math.PI) result -= 2 * Math.PI;
        return result;
    }

    public static Coordinate Centroid(IList<Coordinate> coordinates)
    {
        var sumX = coordinates.Select(c => c.X).Sum();
        var sumY = coordinates.Select(c => c.Y).Sum();
        return new Coordinate(sumX / coordinates.Count, sumY / coordinates.Count);
    }

    /// <summary>
    ///     Negates the vector input length with an exponential curve and adjusted according to maxOutputValue.
    ///     E.g.: maxVectorLenght = 100. maxOutputValue = 10.0: vector in is 75,0, out is -4.8, vector in is 25,0, out is -9.1;
    ///     Used to create the vector that pushes the sheep away from the enemy.
    /// </summary>
    /// <param name="vector">The input vector to adjust</param>
    /// <param name="maxVectorLength">The input vectors max lenght (to be able to adjust the lenght)</param>
    /// <param name="maxOutputValue">The return value max threshold. Output is adjusted to this.</param>
    /// <returns></returns>
    public static Vector2 NegateLengthWithExponentialDecrease(Vector2 vector, double maxVectorLength,
        double maxOutputValue)
    {
        var lenghtAdjusted = LengthWithExponentialDecrease(vector, maxVectorLength, maxOutputValue);
        return Vector2.Negate(lenghtAdjusted);
    }

    public static Vector2 LengthWithExponentialDecrease(Vector2 vector, double maxVectorLength,
        double maxOutputValue)
    {
        var length = ExponentialDecrease(vector.Length() / maxVectorLength, 10.0);
        if (length < 0.0) return Vector2.Zero;
        var norm = Vector2.Normalize(vector);
        return Vector2.Multiply(norm, (float) (length * maxOutputValue));
    }

    /// <summary>
    ///     On x = 0, return value is 1.0, on x = 1.0 return value is 0.0,
    ///     This to make the sheep move faster if the enemy is close.
    /// </summary>
    /// <param name="x">Input factor from 0.0 to 1.0</param>
    /// <param name="curve">Input factor on how the curve should look like</param>
    /// <returns>x after it has been adjusted</returns>
    public static double ExponentialDecrease(double x, double curve)
    {
        if (x < 0.0 || x > 1.0) throw new ArgumentException("x should be a relative factor from 0-1");
        return -1 * ((Math.Pow(curve, x) - 1) / (curve - 1)) + 1;
    }

    public static Vector2 RotateVector(Vector2 vector, double anglesInRadians)
    {
        var newX = vector.X * Math.Cos(anglesInRadians) - vector.Y * Math.Sin(anglesInRadians);
        var newY = vector.X * Math.Sin(anglesInRadians) + vector.Y * Math.Cos(anglesInRadians);
        return new Vector2((float) newX, (float) newY);
    }

    public static bool InRange(Coordinate a, Coordinate b, double under, double over)
    {
        var v = Converter.ToVector2(a, b);
        return Math.Abs(v.Length()) < under && Math.Abs(v.Length()) > over;
    }

    public static bool UnderWithHysteresis(bool value, Coordinate a, Coordinate b, double under, double hysteresis)
    {
        var lenght = Math.Abs(Converter.ToVector2(a, b).Length());
        if (lenght < under) return true;

        if (lenght > under + hysteresis) return false;

        return value;
    }

    public static Coordinate Outcast(IList<Coordinate> coordinates, Coordinate center)
    {
        if (coordinates.Count < 1)
            throw new ArgumentException($"{nameof(coordinates)} must have at least 1 element to find the outcast");

        var outcast = coordinates.First();
        var length = Converter.ToVector2(center, outcast).Length();
        foreach (var coordinate in coordinates)
        {
            var currentCoordinateLenght = Converter.ToVector2(center, coordinate).Length();
            if (currentCoordinateLenght > length)
            {
                outcast = coordinate;
                length = currentCoordinateLenght;
            }
        }

        return outcast;
    }
}