using System.Numerics;
using SheepHerding.Api.Helpers;
using SheepHerding.Api.Services;
using SheepHerding.Api.StateMachine;
using Stateless;

namespace SheepHerding.Api.Entities;

public class DroneOversight : Point
{
    private readonly ILogger _logger;
    private readonly List<AckableCoordinate> _predefinedPathPoints;
    private readonly List<DroneHerder> _herders;
    private double _speed = 10.0;
    private double _herdRadius = 75.0;
    private double _herdAngleInRadians = Math.PI / 2.5;
    private int _pathIndex = 0; // TODO fix this
    private bool _herdInPosesion;
    private Coordinate _current = new(Double.MaxValue, Double.MaxValue);
    private Coordinate _next = new(Double.MaxValue, Double.MaxValue);
    private Coordinate _command = new(0, 0);
    private IList<AckableCoordinate> _commands = new List<AckableCoordinate>();
    private Point _nextPoint = new(0, 0, 0, Double.MaxValue, Double.MaxValue);
    private Point _commandPoint = new(0, 0, 0, Double.MaxValue, Double.MaxValue);
    private MaxRateOfChange _mrcAngle = new();
    private bool _pathAngleCalculationActivated = false;
    private bool _commandInRange;
    private Vector2 _positionCommandVector = Vector2.One;
    private Machine _machine = new();
    private List<Coordinate> _centroids;
    private List<AckableCoordinate> _closestPathPoints;
    private List<AckableCoordinate> _closestPathPointToSheepCentroid;
    private PathCreator _pathCreator;
    private readonly List<Sheep> _sheeps;

    public DroneOversight(ILogger logger, double maxX, double maxY, int id,
        List<AckableCoordinate> predefinedPathPoints, List<DroneHerder> herders, PathCreator pathCreator, List<Sheep> sheeps) :
        base(maxX, maxY, id)
    {
        _logger = logger;
        _predefinedPathPoints = predefinedPathPoints;
        _herders = herders;
        _pathCreator = pathCreator;
        _sheeps = sheeps;
        InitializeStateMachine();
        StartupCalculations();
    }

    private void InitializeStateMachine()
    {
        _machine.ExecuteOnEntry(State.FetchingFirstHerd, () =>
        {
            // _logger.LogInformation("On entry FetchingFirstHerd");
            _centroids = GetSheepHerdCentroids(_sheeps);
            _current = _centroids[0];
            _closestPathPointToSheepCentroid = GetClosesPathPointToClosestSheepCentroid();
            _next = _closestPathPointToSheepCentroid[0];
            // _commandPoint = GetCommandPointV3(_command, _current, _next, 100.0);
            // _command =  _current;
            _commands = _pathCreator.CurvedLineV2(Position, _current, _next);
        });

        _machine.ExecuteOnEntry(State.Waiting, () =>
        {
            // _logger.LogInformation("On entry Waiting");
            if (_current is AckableCoordinate ack)
            {
                // _logger.LogInformation("On entry Waiting ack");
                ack.Ack();
            }

            _closestPathPoints = GetClosesPathPoint();
           // _current = _closestPathPoint[0];
        });

        _machine.ExecuteOnEntry(State.FetchingNewHerd, () =>
        {
            // _logger.LogInformation("On entry FetchingNewHerd");
            _centroids = GetSheepHerdCentroids(_sheeps);
            _current = _centroids.Count > 1 ? _centroids[1] : _centroids[0];
            _closestPathPointToSheepCentroid = GetClosesPathPointToClosestSheepCentroid();
            _next = _closestPathPointToSheepCentroid[0];
            // _commandPoint = GetCommandPointV3(_command, _current, _next, 100.0);
            // _command = _current; //centroids.Count > 1 ? _commandPoint.Position : _current;
        });
        _machine.ExecuteOnEntry(State.FollowPath, () =>
        {
            // _logger.LogInformation("On entry FollowPath");
            _closestPathPoints = GetClosesPathPoint();
            _current = _closestPathPoints[0];
            _next = _closestPathPoints.Count > 1 ? _closestPathPoints[1] : _closestPathPoints[0];
            // _commandPoint = GetCommandPointV3(_command, _current, _next, 150.0);
            // _command = _current; //_commandPoint.Position;
            _commands = _pathCreator.CurvedLineV2(Position, _current, _next);
        });
        _machine.ExecuteOnEntry(State.RecollectSheep, () =>
        {
            // _logger.LogInformation("On entry RecollectSheep");
            _centroids = GetSheepHerdCentroids(_sheeps);
            _current = _centroids[0];
            _closestPathPointToSheepCentroid = GetClosesPathPointToClosestSheepCentroid();
            _next = _closestPathPointToSheepCentroid[0];
            // _command = _current;
        });
        _machine.ExecuteOnEntry(State.Finished, () => { _logger.LogInformation("On entry Finished"); });
    }

    private void StartupCalculations()
    {
        _centroids = GetSheepHerdCentroids(_sheeps);
        _closestPathPoints = GetClosesPathPoint();
        _closestPathPointToSheepCentroid = GetClosesPathPointToClosestSheepCentroid();
    }

    private List<AckableCoordinate> GetClosesPathPoint()
    {
        var points = _predefinedPathPoints
            .Where(p => (!p.Accessed && p.PathIndex >= _pathIndex) || (p.Accessed && p.PathIndex > _pathIndex))
            .OrderBy(p => p.PathIndex)
            .ToList();
        _pathIndex = points == null || points.Count < 1
            ? _predefinedPathPoints.Count - 1
            : points[0].PathIndex;
        return points;
    }

    private List<AckableCoordinate> GetClosesPathPointToClosestSheepCentroid()
    {
        var points = _predefinedPathPoints
            .Where(p => (!p.Accessed && p.PathIndex >= _pathIndex) || (p.Accessed && p.PathIndex > _pathIndex))
            .OrderBy(p => Converter.ToVector2(p, _centroids[0]).Length())
            .ToList();
        _pathIndex = points == null || points.Count < 1
            ? _predefinedPathPoints.Count - 1
            : points[0].PathIndex;
        return points;
    }

    private List<Coordinate> GetSheepHerdCentroids(List<Sheep> sheeps)
    {
        var groups = Clustering.Cluster(sheeps.Where(s => !s.IsInsideFinishZone()).ToList<Point>(), 100.0);
        return groups
            .Select(g =>
                Calculator.Centroid(g.Select(gg => new Coordinate(gg.Position.X, gg.Position.Y)).ToList()))
            .OrderBy(g => Converter.ToVector2(Position, g).Length())
            .ToList();
    }

    public (int pathIndex, List<Coordinate> centroids, Coordinate current, Coordinate next, string state,
        IList<Coordinate> points) UpdatePosition(bool disableHerders, double dt, double[] settings)
    {
        // _herdRadius = settings[0]; //largestDistance; // settings[0];
        _herdAngleInRadians = settings[1] == 0 ? _herdAngleInRadians : settings[1];
        _speed = settings[2];

        // -- Calculations

        // Clustering different herds
        _centroids = GetSheepHerdCentroids(_sheeps);

        // Move the drone

        _closestPathPointToSheepCentroid = GetClosesPathPointToClosestSheepCentroid();

        // -- Setting states
        _commandInRange = Calculator.UnderWithHysteresis(_commandInRange, Position, _command, 5.0, 1.0);
        var currentInRange = Calculator.InRange(Position, _current, 45.0, 0.0);
        var sheepCentroidInRange = Calculator.InRange(Position, _centroids[0], 45.0, 0.0);
        var sheepCentroidOutOfRange = Calculator.InRange(Position, _centroids[0], 100.0, 50.0);
        var closestPathPointOutOfRange = Calculator.InRange(Position, _closestPathPoints[0], 10000.0, 10.0);


        var lenghtToNextHerd =
            _centroids.Count < 2 ? int.MaxValue : Converter.ToVector2(Position, _centroids[1]).Length();
        var lenghtToNextPathPoint = Converter.ToVector2(Position, _closestPathPoints[0]).Length();

        var lenghtFromCurrentToHerd =
            _centroids.Count < 2 ? int.MaxValue : Converter.ToVector2(_current, _centroids[1]).Length();
        var lenghtFromNextToHerd =
            _centroids.Count < 2 ? int.MaxValue : Converter.ToVector2(_next, _centroids[1]).Length();

        _machine.Fire(State.FetchingFirstHerd, Trigger.NewHerdCollected, () => currentInRange);
        _machine.Fire(State.FetchingNewHerd, Trigger.NewHerdCollected, () => currentInRange);
        _machine.Fire(State.FollowPath, Trigger.SheepEscaped, () => sheepCentroidOutOfRange);
        _machine.Fire(State.FollowPath, Trigger.NewHerdInRange, () => lenghtFromCurrentToHerd < lenghtFromNextToHerd);
        _machine.Fire(State.FollowPath, Trigger.CommandsExecuted, () => _commands.All(c => c.Accessed));
        _machine.Fire(State.Waiting, Trigger.PathPointOutOfRange, () => closestPathPointOutOfRange);
        _machine.Fire(State.RecollectSheep, Trigger.SheepCaptured, () => sheepCentroidInRange);
        _machine.Fire(State.Start, Trigger.Start, () => true);


        // Write out
        if (_commandInRange && _command is AckableCoordinate ack)
        {
            ack.Ack();
        }

        _nextPoint = new Point(0, 0, 0, _current.X, _current.Y);
        var c = _commands.FirstOrDefault(c => !c.Accessed);
        if (c != null)
        {
            _command = c;
            _positionCommandVector = Converter.ToVector2(Position, new Coordinate(_command.X, _command.Y));
        }

        var pathPoint = new Point(0, 0, 0, Position.X, Position.Y);
        pathPoint.Force = _positionCommandVector;


        var force = Vector2.Multiply(Vector2.Normalize(_positionCommandVector), 4.0f);
        Force = Vector2.Multiply(force, 10); // For visualization purposes only
        Position.Update(Position.X + (force.X * (dt / 100)), Position.Y + (force.Y * (dt / 100)));

        // After updating this position, update the herders
        var h0 = Vector2.Multiply(Vector2.Negate(Vector2.Normalize(_positionCommandVector)), (float) _herdRadius);
        var h1 = Calculator.RotateVector(h0, _herdAngleInRadians);
        var h2 = Calculator.RotateVector(h0, -_herdAngleInRadians);

        if (disableHerders)
            return (_pathIndex, _centroids.ToList(), _command, _next, _machine.State.ToString(),
                new List<Coordinate>());
        _herders[0].UpdatePosition(dt, new Coordinate(Position.X + h0.X, Position.Y + h0.Y));
        _herders[1].UpdatePosition(dt, new Coordinate(Position.X + h1.X, Position.Y + h1.Y));
        _herders[2].UpdatePosition(dt, new Coordinate(Position.X + h2.X, Position.Y + h2.Y));
        var pointList = new List<Coordinate> {pathPoint.Position, _nextPoint.Position, _commandPoint.Position};
        pointList.AddRange(_commands);
        return (_pathIndex, _centroids.ToList(), _command, _next, _machine.State.ToString(), pointList);
    }

    public double GetHerdingCircleRadius()
    {
        return _herdRadius;
    }

    private Point GetCommandPointV1(Vector2 pathVector, Coordinate current, Coordinate next, double reductionStart)
    {
        var nextVector = Converter.ToVector2(current, next);
        var currentVector = Converter.ToVector2(Position, current);
        var pathVectorNorm = Vector2.Normalize(pathVector);
        double reduction;
        if (currentVector.Length() > reductionStart && pathVector.Length() > reductionStart)
        {
            reduction = reductionStart;
        }
        else if (currentVector.Length() > pathVector.Length())
        {
            reduction = pathVector.Length();
        }
        else
        {
            reduction = currentVector.Length();
        }

        var rotatedNormNextVector = Vector2.Normalize(Calculator.RotateVector(nextVector, Math.PI / 2));
        var adjustedNextVector = Vector2.Multiply(Vector2.Normalize(rotatedNormNextVector), (float) reduction);

        var command = new Point(0, 0, 0, current.X + adjustedNextVector.X, current.Y + adjustedNextVector.Y);
        command.Force = Vector2.Negate(adjustedNextVector);
        return command;
    }

    private Point GetCommandPointV2(Vector2 pathVector, Coordinate current, Coordinate next, double reductionStart)
    {
        var currentNextVector = Converter.ToVector2(current, next);
        var positionCurrentVector = Converter.ToVector2(Position, current);

        var angles = Calculator.AngleInRadiansLimited(positionCurrentVector, currentNextVector);

        var pathVectorNorm = Vector2.Normalize(pathVector);
        double reduction;
        if (positionCurrentVector.Length() * 2 > reductionStart && pathVector.Length() * 2 > reductionStart)
        {
            reduction = reductionStart;
        }
        else if (positionCurrentVector.Length() > pathVector.Length())
        {
            reduction = pathVector.Length() * 2;
        }
        else
        {
            reduction = positionCurrentVector.Length() * 2;
        }

        var rotatedNormNextVector = Vector2.Normalize(Calculator.RotateVector(currentNextVector, angles));
        var adjustedNextVector = Vector2.Multiply(Vector2.Normalize(rotatedNormNextVector), (float) reduction);

        var command = new Point(0, 0, 0, current.X + adjustedNextVector.X, current.Y + adjustedNextVector.Y);
        command.Force = Vector2.Negate(adjustedNextVector);
        return command;
    }

    private Point GetCommandPointV3(Coordinate previousCommand, Coordinate current, Coordinate next,
        double reductionStart)
    {
        var commandVector = Vector2.Multiply(Calculator.GetCommandVector(Position, current, next), 100.0f);

        var command = new Point(0, 0, 0, current.X + commandVector.X, current.Y + commandVector.Y);
        command.Force = Vector2.Negate(commandVector);
        return command;
    }
}