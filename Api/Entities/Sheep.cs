using System.Numerics;
using SheepHerding.Api.Helpers;

namespace SheepHerding.Api.Entities;

public class Sheep : Point
{
    private readonly IList<Sheep> _friendlies;
    private readonly IList<DroneHerder> _enemies;
    private readonly Coordinate _finish;
    private readonly double _neighborToCloseStartMoveThreshold = 25.0;
    private readonly double _neighborToFarStartToMoveThreshold = 150.0;
    private readonly double _centroidOfHerdToFarStartMoveThreshold = 100.0;
    private readonly double _centroidOfHerdToFarEndMoveThreshold = 200.0;
    private readonly double _enemyToCloseStartMoveThreshold = 100.0;
    private readonly double _maxSpeed = 100.0;
    private readonly ILogger _logger;

    public Sheep(ILogger logger, double maxX, double maxY, int id, IList<Sheep> friendlies, IList<DroneHerder> enemies,
        Coordinate finish) : base(maxX, maxY, id)
    {
        _logger = logger;
        _friendlies = friendlies;
        _enemies = enemies;
        _finish = finish;
    }

    public void UpdatePosition(double forceAdjustment)
    {
        var personalSpaceForce = 3.0f;
        var holdTogetherForce = 1.0f;
        var runAwayForce = 1.0f;
        var force = new Vector2(0, 0);
        
        // Personal space - dont have sheeps walk on top of each other
        var close = _friendlies.Where(s
            => s.Id != Id && Converter.ToVector2(Position, s.Position).Length() < _neighborToCloseStartMoveThreshold)
            .MinBy(s => Converter.ToVector2(Position, s.Position).Length());

        if (close != null)
        {
            var sheepVclose = Converter.ToVector2Negated(Position, new Coordinate(close.Position.X, close.Position.Y));
            var normalized = Vector2.Normalize(Vector2.Add(force, sheepVclose));
            force = Vector2.Multiply(normalized, personalSpaceForce);
            // _logger.LogInformation($"Sheep '{Id}' needs personal space");
        }

        // Hold together as a herd
        var neighbours = _friendlies
            .Where(s => s.Id != Id
                        && Converter.ToVector2(Position, s.Position).Length() > 100 
                        && Converter.ToVector2(Position, s.Position).Length() < 200)
            .OrderBy(s => Converter.ToVector2(Position, s.Position).Length())
            .Take(5);
        if (neighbours.Any())
        {
            var list = neighbours.Select(s => s.Position).ToList();
            list.Add(Position);
            var farCentroid = Calculator.Centroid(list);
            var sheepVfar = Converter.ToVector2(Position, new Coordinate(farCentroid.X, farCentroid.Y));
            var normalized = Vector2.Normalize(Vector2.Add(force, sheepVfar));
            force = Vector2.Divide(normalized, 1.0f);
            // _logger.LogInformation($"Sheep '{Id}' hold together as a herd");

        }
        
        // Grazing
        var randomDirection = Calculator.RotateVector(Vector2.One, new Random().NextDouble() * Math.PI * 2);
        force = Vector2.Add(force, randomDirection);

        // Enemies
        var sheepVenemy = _enemies.Select(e => Converter.ToVector2Negated(Position, e.Position));
        var minLenght = sheepVenemy.Select(v => v.Length()).Min();
        if (minLenght <= _enemyToCloseStartMoveThreshold)
        {
            foreach (var enemy in sheepVenemy)
            {
                var flipped = Calculator.FlipExLength(enemy, 100.0);
                var flippedReduced = Vector2.Divide(flipped, 10);
                var flippedRandomRotated =
                    Calculator.RotateVector(flippedReduced, new Random().NextDouble() * (Math.PI/4));
                force = Vector2.Add(force, flippedRandomRotated);
            }
        }

        Force = Vector2.Multiply(force, 10); // For visualization purposes only
        Position.Update(Position.X + (force.X * forceAdjustment), Position.Y + (force.Y * forceAdjustment));
    }

    public bool IsInsideFinishZone()
    {
        return Position.X >= _finish.X && Position.Y >= _finish.Y;
    }
}