using System.Numerics;
using SheepHerding.Api.Helpers;

namespace SheepHerding.Api.Entities;

public class Sheep : Point
{
    private readonly IList<Sheep> _friendlies;
    private readonly IList<DroneHerder> _enemies;
    private double _closeThreshold = 10.0;

    public Sheep(double maxX, double maxY, int id, IList<Sheep> friendlies, IList<DroneHerder> enemies, DroneOversight oversight) : base(maxX, maxY, id)
    {
        _friendlies = friendlies;
        _enemies = enemies;
    }

    public override void UpdatePosition(Coordinate sheepCentroid, double dt, double[] settings)
    {
        var force = new Vector2(0, 0);
        
        var close = _friendlies.Where(s => s.Id != Id &&
            (Math.Abs(s.Position.X - Position.X) <= _closeThreshold) && (Math.Abs(s.Position.Y - Position.Y) <= _closeThreshold)).ToList();
        if (close.Any())
        {
            var list = close.Select(s => s.Position).ToList();
            list.Add(Position);
            var closeCentroid = Calculator.Centroid(list);
            var sheepVclose = new Vector2(Convert.ToSingle(Position.X - closeCentroid.x), 
                Convert.ToSingle(Position.Y - closeCentroid.y));
            force = Vector2.Add(force, sheepVclose);
        }

        var sheepVcentroid = new Vector2(Convert.ToSingle(Position.X - sheepCentroid.X), Convert.ToSingle(Position.Y - sheepCentroid.Y));
        var sheepVcentroidReduced = Vector2.Divide(sheepVcentroid, 10);
        if (sheepVcentroid.Length() > 50.0)
        {
            var negated = Vector2.Negate(sheepVcentroidReduced);
            force = Vector2.Add(force, negated);
        }

        var sheepVenemy = _enemies.Select(e 
            => new Vector2(Convert.ToSingle(Position.X - e.Position.X),
            Convert.ToSingle(Position.Y - e.Position.Y)));
        var minLenght = sheepVenemy.Select(v => v.Length()).Min();
        var maxLenght = sheepVenemy.Select(v => v.Length()).Max();
        if (minLenght <= 100.0)
        {
            foreach (var enemy in sheepVenemy) // TODO this is not working
            {
                var flipped = Calculator.FlipLength(enemy, 100.0);
                var flippedReduced = Vector2.Divide(flipped, 10);
                force = Vector2.Add(force, flippedReduced);
            }
        }

        Force = Vector2.Multiply(force, 10); // For visualization purposes only
        Position.Update(Position.X + (force.X * (dt/100)), Position.Y + (force.Y * (dt/100)));
    }
    
}

public abstract class Point
{
    internal readonly int Id;
    internal readonly Coordinate Position;
    internal Vector2 Force = Vector2.Zero;
    private readonly double _maxX;
    private readonly double _maxY;

    public Point(double maxX, double maxY, int id)
    {
        Position = new Coordinate(0, 0);
        _maxX = maxX;
        _maxY = maxY;
        Id = id;
    }

    public abstract void UpdatePosition(Coordinate sheepCentroid, double dt, double[] settings);

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