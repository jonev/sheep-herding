using System.Numerics;
using SheepHerding.Api.Helpers;

namespace SheepHerding.Api.Entities;

public class Sheep : Point
{
    private readonly IList<Sheep> _friendlies;
    private readonly IList<DroneHerder> _enemies;
    private readonly Coordinate _finish;
    private readonly double _neighborToCloseStartMoveThreshold = 15.0;
    private readonly double _neighborToFarStartToMoveThreshold = 150.0;
    private readonly double _centroidOfHerdToFarStartMoveThreshold = 100.0;
    private readonly double _centroidOfHerdToFarEndMoveThreshold = 200.0;
    private readonly double _enemyToCloseStartMoveThreshold = 100.0;
    private readonly double _maxSpeed = 100.0;

    public Sheep(double maxX, double maxY, int id, IList<Sheep> friendlies, IList<DroneHerder> enemies, Coordinate finish) : base(maxX, maxY, id)
    {
        _friendlies = friendlies;
        _enemies = enemies;
        _finish = finish;
    }

    public void UpdatePosition(double dt)
    {
        var force = new Vector2(0, 0);
        
        // Personal space - dont have sheeps walk on top of each other
        var close = _friendlies.Where(s 
                => s.Id != Id 
                   && Calculator.InRange(Position, s.Position, _neighborToCloseStartMoveThreshold, 0.0))
            .ToList();
        
        if (close.Any())
        {
            var list = close.Select(s => s.Position).ToList();
            list.Add(Position);
            var closeCentroid = Calculator.Centroid(list);
            var sheepVclose = Converter.ToVector2Negated(Position, new Coordinate(closeCentroid.X, closeCentroid.Y));
            force = Vector2.Multiply(Vector2.Add(force, sheepVclose), 3.0f);
        }

        // Hold together as a herd
        var neighbours = _friendlies
            .Where(s => s.Id != Id
                        && Calculator.InRange(Position, s.Position,
                            100.0,
                            50.0))
            .OrderBy(s => Converter.ToVector2(Position, s.Position).Length())
            .Take(5);
            if (neighbours.Count() < 5)
        {
            var list = neighbours.Select(s => s.Position).ToList();
            list.Add(Position);
            var farCentroid = Calculator.Centroid(list);
            var sheepVfar = Converter.ToVector2(Position, new Coordinate(farCentroid.X, farCentroid.Y));
            force = Vector2.Divide(Vector2.Add(force, sheepVfar), 20.0f);
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

    public bool IsInsideFinishZone()
    {
        return Position.X >= _finish.X && Position.Y >= _finish.Y;
    }

}