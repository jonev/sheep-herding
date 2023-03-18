using System.Numerics;
using SheepHerding.Api.Helpers;

namespace SheepHerding.Api.Entities;

public class Sheep : Point
{
    private readonly IList<DroneHerder> _enemies;
    private readonly Coordinate _finish;
    private readonly IList<Sheep> _friendlies;
    private readonly ILogger _logger;
    private readonly int[] _randomAngleSeeds = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15};
    private readonly int _randomSeed;
    private readonly SheepSettings _settings;
    private readonly PathCoordinator _terrainPath;
    private bool _pathCoordinateInRange;
    private double _randomAngle;
    private int _randomAngleSeedsIndex;
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

    // Max speed => Enemy = 1, PersonalSpace = 0.1, To far from herd = 0.1 => 1.2

    public void UpdatePosition()
    {
        _scanIndex++;
        var force = new Vector2(0, 0);

        // Personal space - dont have sheeps walk on top of each other
        var neighborsThatAreToClose = _friendlies.Where(s
                => s.Id != Id && Converter.ToVector2(Position, s.Position).Length() <
                _settings.NeighborToCloseStartMoveThreshold)
            .ToList();

        foreach (var neighbor in neighborsThatAreToClose)
        {
            var flipped = Calculator.NegateLengthWithExponentialDecrease(
                Converter.ToVector2(Position, neighbor.Position),
                _settings.NeighborToCloseStartMoveThreshold,
                _settings.PersonalSpaceForce);
            force = Vector2.Add(force, flipped);
        }

        // Hold together as a herd
        var neighbours = _friendlies
            .Where(s => s.Id != Id
                        && Converter.ToVector2(Position, s.Position).Length() >
                        _settings.CentroidOfHerdToFarStartMoveTowardHerdThreshold
                        && Converter.ToVector2(Position, s.Position).Length() <
                        _settings.CentroidOfHerdToFarEndMoveThreshold)
            .OrderBy(s => Converter.ToVector2(Position, s.Position).Length())
            .Take(5);
        if (neighbours.Any())
        {
            var list = neighbours.Select(s => s.Position).ToList();
            list.Add(Position);
            var centroidOfTheClosestSheepHerd = Calculator.Centroid(list);
            var sheepToCentroidVectorAdjusted = Calculator.LengthWithExponentialDecrease(
                Converter.ToVector2(Position,
                    new Coordinate(centroidOfTheClosestSheepHerd.X, centroidOfTheClosestSheepHerd.Y)),
                _settings.CentroidOfHerdToFarEndMoveThreshold,
                _settings.HoldTogetherForce);
            force = Vector2.Add(force, sheepToCentroidVectorAdjusted);
        }

        // Grazing
        // var randomDirection = Calculator.RotateVector(Vector2.One, new Random().NextDouble() * Math.PI * 2);
        // force = Vector2.Add(force, randomDirection);

        // Enemies - Herding
        var toCloseEnemiesList = _enemies
            .Where(e => Converter.ToVector2(Position, e.Position).Length() < _settings.EnemyToCloseStartMoveThreshold)
            .ToList();
        // TODO this is not working well after introducing the new path as a tree
        if (_scanIndex % _settings.RandomAngleUpdateDelayFactor == 0)
        {
            // Dont update so often
            _randomAngle = (new Random(_randomAngleSeeds[_randomAngleSeedsIndex % 15]).NextDouble() - 0.5) *
                           _settings.RandomAngleAddedToForce;
            _randomAngleSeedsIndex++;
            // if (Id == 1)
            //     _logger.LogInformation($"Random angle = {_randomAngle}");
        }

        var enemyClose = toCloseEnemiesList.Any();
        foreach (var enemy in toCloseEnemiesList)
        {
            var flipped = Calculator.NegateLengthWithExponentialDecrease(
                Converter.ToVector2(Position, enemy.Position),
                _settings.EnemyToCloseStartMoveThreshold,
                _settings.RunAwayForce);
            force = Vector2.Add(force, flipped);
            // _logger.LogInformation(
            //     $"Enemy: {flipped.Length()},{flipped.X},{flipped.Y}, force: {force.Length()},{force.X},{force.Y}");
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
        if (enemyClose && _terrainPath.IntersectionApproaching(Position))
        {
            var currentForSheep = _terrainPath.GetCurrent(PATH_EXECUTER.SHEEP);
            var adjustedSheepVPath = Vector2.Multiply(Vector2.Normalize(sheepVpath), 1.1f);
            // _logger.LogInformation($"Sheep drawn: {adjustedSheepVPath.Length()}, {currentForSheep}");
            force = Vector2.Add(force, adjustedSheepVPath);
        }

        if (enemyClose && sheepVpath.Length() > 100.0 && !_terrainPath.IntersectionApproaching(Position))
            _terrainPath.Ack(PATH_EXECUTER.SHEEP);

        var rotated = Calculator.RotateVector(force, _randomAngle);

        Force = Vector2.Multiply(rotated, 10); // For visualization purposes only
        Position.Update(Position.X + rotated.X, Position.Y + rotated.Y);
    }

    public bool IsInsideFinishZone()
    {
        return Position.X >= _finish.X && Position.Y >= _finish.Y;
    }
}