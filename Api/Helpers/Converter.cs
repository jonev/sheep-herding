using System.Numerics;

namespace SheepHerding.Api.Helpers;

public static class Converter
{
    public static Vector2 ToVector2(double x1, double y1, double x2, double y2)
    {
        return new Vector2(Convert.ToSingle(x2 - x1),
            Convert.ToSingle(y2 - y1));
    }
}