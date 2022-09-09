using System.Numerics;
using SheepHerding.Api.Helpers;

namespace SheepHerding.Api.Entities;

public class Sheep : Point
{
    private readonly IList<Sheep> _friendlies;
    private readonly IList<DroneHerder> _enemies;
    private readonly double _neighborToCloseStartMoveThreshold = 10.0;
    private readonly double _centroidOfHerdToFarStartMoveThreshold = 10.0;
    private readonly double _centroidOfHerdToFarEndMoveThreshold = 150.0;
    private readonly double _enemyToCloseStartMoveThreshold = 100.0;
    private readonly double _maxSpeed = 100.0;

    public Sheep(double maxX, double maxY, int id, IList<Sheep> friendlies, IList<DroneHerder> enemies, DroneOversight oversight) : base(maxX, maxY, id)
    {
        _friendlies = friendlies;
        _enemies = enemies;
    }

    public void UpdatePosition(Coordinate sheepCentroid, double dt)
    {
        var force = new Vector2(0, 0);
        
        // Personal space - dont have sheeps walk on top of each other
        var close = _friendlies.Where(s => s.Id != Id &&
            (Math.Abs(s.Position.X - Position.X) <= _neighborToCloseStartMoveThreshold) && (Math.Abs(s.Position.Y - Position.Y) <= _neighborToCloseStartMoveThreshold)).ToList();
        
        if (close.Any())
        {
            var list = close.Select(s => s.Position).ToList();
            list.Add(Position);
            var closeCentroid = Calculator.Centroid(list);
            var sheepVclose = Converter.ToVector2Negated(Position, new Coordinate(closeCentroid.x, closeCentroid.y));
            force = Vector2.Multiply(Vector2.Add(force, sheepVclose), 4.0f);
        }

        // Hold together as a herd
        var sheepVcentroid = Converter.ToVector2(Position, sheepCentroid);
        if (sheepVcentroid.Length() > _centroidOfHerdToFarStartMoveThreshold && sheepVcentroid.Length() < _centroidOfHerdToFarEndMoveThreshold)
        {
            force = Vector2.Add(force,
                Calculator.Pull(sheepVcentroid, sheepVcentroid.Length() / _centroidOfHerdToFarEndMoveThreshold, 10.0));
        }
        
        // Enemies
        var sheepVenemy = _enemies.Select(e => Converter.ToVector2Negated(Position, e.Position));
        var minLenght = sheepVenemy.Select(v => v.Length()).Min();
        if (minLenght <= _enemyToCloseStartMoveThreshold)
        {
            foreach (var enemy in sheepVenemy)
            {
                var flipped = Calculator.FlipExLength(enemy, 100.0);
                var flippedReduced = Vector2.Divide(flipped, 10);
                force = Vector2.Add(force, flippedReduced);
            }
        }

        Force = Vector2.Multiply(force, 10); // For visualization purposes only
        Position.Update(Position.X + (force.X * (dt/100)), Position.Y + (force.Y * (dt/100)));
    }
    
}