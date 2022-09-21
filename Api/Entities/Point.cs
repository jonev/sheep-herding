using System.Numerics;

namespace SheepHerding.Api.Entities;

public class Point
{
    internal readonly int Id;
    internal readonly Coordinate Position;
    internal Vector2 Force = Vector2.Zero;
    internal readonly double MaxX;
    internal readonly double MaxY;

    public Point(double maxX, double maxY, int id, double x = 0, double y = 0)
    {
        Position = new Coordinate(x, y);
        MaxX = maxX;
        MaxY = maxY;
        Id = id;
    }

    public void Set(Coordinate next)
    {
        Position.Update(next);
    }

    public void SetRandomStartPosition()
    {
        var r = new Random();
        Position.Update(r.NextDouble() * MaxX, r.NextDouble() * MaxY);
    }
}