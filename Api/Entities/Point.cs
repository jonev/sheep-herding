using System.Numerics;

namespace SheepHerding.Api.Entities;

public abstract class Point
{
    internal readonly int Id;
    internal readonly Coordinate Position;
    internal Vector2 Force = Vector2.Zero;
    internal readonly double MaxX;
    internal readonly double MaxY;

    public Point(double maxX, double maxY, int id)
    {
        Position = new Coordinate(0, 0);
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