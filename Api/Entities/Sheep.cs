using System.Numerics;
using SheepHerding.Api.Helpers;

namespace SheepHerding.Api.Entities;

public class Sheep : Point
{
    private readonly IList<DroneHerder> _enemies;
    private readonly Coordinate _finish;
    private readonly IList<Sheep> _friendlies;
    private readonly ILogger _logger;
    private readonly int _randomSeed;
    private readonly SheepSettings _settings;
    private readonly PathCoordinator _terrainPath;
    private bool _pathCoordinateInRange;
    private double _randomAngle;
    private int _scanIndex;

    public Sheep(ILogger logger, int id, SheepSettings settings, IList<Sheep> friendlies, IList<DroneHerder> enemies,
        Coordinate finish, PathCoordinator terrainPath, int randomSeed) : base(id)
    {
        _logger = logger;
        _settings = settings;
        _friendlies = friendlies;
        _enemies = enemies;
        _finish = finish;
        _terrainPath = terrainPath;
        _randomSeed = randomSeed;
    }

    public void UpdatePosition(double forceAdjustment)
    {
        _scanIndex++;
        var force = new Vector2(0, 0);

        // Personal space - dont have sheeps walk on top of each other
        var close = _friendlies.Where(s
                => s.Id != Id && Converter.ToVector2(Position, s.Position).Length() <
                _settings.NeighborToCloseStartMoveThreshold)
            .MinBy(s => Converter.ToVector2(Position, s.Position).Length());

        if (close != null)
        {
            var sheepVclose = Converter.ToVector2Negated(Position, new Coordinate(close.Position.X, close.Position.Y));
            var normalized = Vector2.Normalize(Vector2.Add(force, sheepVclose));
            force = Vector2.Multiply(normalized, _settings.PersonalSpaceForce);
            // force = Calculator.AddWithForceFactor(
            //     force, 
            //     sheepVclose, 
            //     _settings.PersonalSpaceForce);
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
            // force = Calculator.AddWithForceFactor(
            //     force, 
            //     sheepVfar, 
            //     _settings.HoldTogetherForce);
            // _logger.LogInformation($"Sheep '{Id}' hold together as a herd");
        }

        // Grazing
        // var randomDirection = Calculator.RotateVector(Vector2.One, new Random().NextDouble() * Math.PI * 2);
        // force = Vector2.Add(force, randomDirection);

        // Enemies - Herding
        var sheepVenemy = _enemies.Select(e => Converter.ToVector2Negated(Position, e.Position));
        var minLenght = sheepVenemy.Select(v => v.Length()).Min();
        // TODO this is not working well after introducing the new path as a tree
        // if (_randomAngle == 0.0 || _scanIndex % _settings.RandomAngleUpdateDelayFactor == 0)
        // {
        //     // Dont update so often
        //     _randomAngle = (new Random(_randomSeed).NextDouble() - 0.5) * _settings.RandomAngleRange;
        //     _logger.LogInformation($"Random angle = {_randomAngle}");
        // }
        _randomAngle = 0.0;
        var enemyClose = minLenght <= _settings.EnemyToCloseStartMoveThreshold;
        if (enemyClose)
            foreach (var enemy in sheepVenemy)
            {
                var flipped = Calculator.FlipExLength(enemy, 100.0);
                var flippedReduced = Vector2.Divide(flipped, 10);
                // Add random angle to make the herding more challenging
                var flippedRandomRotated =
                    Calculator.RotateVector(flippedReduced, _randomAngle);
                force = Vector2.Add(force, flippedRandomRotated);
                // force = Calculator.AddWithForceFactor(
                //     force, 
                //     flippedRandomRotated, 
                //     _settings.RunAwayForce);
            }

        // Drawn towards the path
        _pathCoordinateInRange = Calculator.UnderWithHysteresis(
            _pathCoordinateInRange,
            Position,
            _terrainPath.GetCurrent(PATH_EXECUTER.SHEEP),
            10.0,
            1.0);
        if (_pathCoordinateInRange)
            // _logger.LogInformation($"Sheep ack path coordinate");
            _terrainPath.Ack(PATH_EXECUTER.SHEEP);

        var sheepVpath = Converter.ToVector2(Position, _terrainPath.GetCurrent(PATH_EXECUTER.SHEEP));
        if (enemyClose) // && sheepVpath.Length() < 200.0)
        {
            var sheepVpathNorm = Vector2.Normalize(sheepVpath);
            force = Vector2.Add(force, Vector2.Multiply(sheepVpathNorm, 8.0f));
        }

        Force = Vector2.Multiply(force, 10); // For visualization purposes only
        Position.Update(Position.X + force.X * forceAdjustment, Position.Y + force.Y * forceAdjustment);
        // _logger.LogInformation($"Sheep force, {force.Length()}, {force.X}, {force.Y}");
        // force = Vector2.Multiply(force, _settings.SpeedAdjustment);
        // Position.Update(Position.X + force.X, Position.Y + force.Y);
    }

    public bool IsInsideFinishZone()
    {
        return Position.X >= _finish.X && Position.Y >= _finish.Y;
    }
}