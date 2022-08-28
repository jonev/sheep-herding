using System.Numerics;
using SheepHerding.Api.Helpers;

namespace SheepHerding.Api.Entities;

public class Sheep : Point
{
    private readonly IList<Sheep> _friendlies;
    private readonly Drone _enemy;
    internal Vector2 Force = Vector2.Zero;
    private double _closeThreshold = 20.0;

    public Sheep(double maxX, double maxY, int id, IList<Sheep> friendlies, Drone enemy) : base(maxX, maxY, id)
    {
        _friendlies = friendlies;
        _enemy = enemy;
    }

    public override void UpdatePosition(Coordinate sheepCentroid, double dt)
    {
        var force = new Vector2(0, 0);
        var sheepVenemy = new Vector2(Convert.ToSingle(Position.X - _enemy.Position.X), Convert.ToSingle(Position.Y - _enemy.Position.Y));
        var sheepVcentroid = new Vector2(Convert.ToSingle(Position.X - sheepCentroid.X), Convert.ToSingle(Position.Y - sheepCentroid.Y));
        var sheepVenemyReduced = Vector2.Divide(sheepVenemy, 10);
        var sheepVcentroidReduced = Vector2.Divide(sheepVcentroid, 10);
        
        var close = _friendlies.Where(s => s.Id != Id &&
            (Math.Abs(s.Position.X - Position.X) <= _closeThreshold) && (Math.Abs(s.Position.Y - Position.Y) <= _closeThreshold)).ToList();
        if (close.Any())
        {
            var list = close.Select(s => s.Position).ToList();
            list.Add(Position);
            var closeCentroid = Calculator.Centroid(list);
            var sheepVclose = new Vector2(Convert.ToSingle(Position.X - closeCentroid.x), 
                Convert.ToSingle(Position.Y - closeCentroid.y));
            var sheepVcloseReduced = Vector2.Divide(sheepVclose, 10);
            force = Vector2.Add(force, sheepVcloseReduced);
        }

        if (sheepVcentroid.Length() > 50.0)
        {
            var negated = Vector2.Negate(sheepVcentroidReduced);
            force = Vector2.Add(force, negated);
        }
        
        if (sheepVenemy.Length() <= 100.0)
        {
            force = Vector2.Add(force, sheepVenemyReduced);
        }

        Force = Vector2.Multiply(force, 100); // For visualization purposes only
        Position.Update(Position.X + (force.X * (dt/100)), Position.Y + (force.Y * (dt/100)));
    }
    
}
public class Drone : Point
{
    public Drone(double maxX, double maxY, int dt) : base(maxX, maxY, dt)
    {
    }

    public override void UpdatePosition(Coordinate sheepCentroid, double dt)
    {
        throw new NotImplementedException();
    }
}

public abstract class Point
{
    internal readonly int Id;
    internal readonly Coordinate Position;
    internal readonly Vector2 Heading = Vector2.Zero;
    private readonly double _maxX;
    private readonly double _maxY;

    public Point(double maxX, double maxY, int id)
    {
        Position = new Coordinate(0, 0);
        _maxX = maxX;
        _maxY = maxY;
        Id = id;
    }

    public abstract void UpdatePosition(Coordinate sheepCentroid, double dt);

    public void Set(Coordinate next)
    {
        Position.Update(next);
    }

    public void SetRandomStartPosition()
    {
        var r = new Random();
        Position.Update(r.NextDouble() * _maxX, r.NextDouble() * _maxY);
    }
}