using System.Numerics;

namespace SheepHerding.Api.Entities;

public class Point
{
    internal readonly int Id;
    internal readonly Coordinate Position;
    internal Vector2 Force = Vector2.Zero;

    public Point(int id, double x = 0, double y = 0)
    {
        Position = new Coordinate(x, y);
        Id = id;
    }

    public void Set(Coordinate next)
    {
        Position.Update(next);
    }
}