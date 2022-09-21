using System.Numerics;
using SheepHerding.Api.Helpers;
using SheepHerding.Api.StateMachine;
using SheepHerding.Api.Workers;
using Stateless;

namespace SheepHerding.Api.Entities;

public class DroneOversight : Point
{
    private readonly StateMachine<State,Trigger> _machine = new (State.FetchingFirstHerd);

    private readonly ILogger<Worker> _logger;
    private readonly List<AckableCoordinate> _predefinedPathPoints;
    private readonly List<DroneHerder> _herders;
    private double _speed = 10.0;
    private double _herdRadius = 75.0;
    private double _herdAngleInRadians = Math.PI / 3;
    private int _pathIndex = 0; // TODO fix this
    private bool _herdInPosesion;
    private Coordinate _current = new (Double.MaxValue, Double.MaxValue);
    private Coordinate _next = new (Double.MaxValue, Double.MaxValue);
    private Coordinate _command = new (Double.MaxValue, Double.MaxValue);
    private Point _nextPoint = new (0,0, 0, Double.MaxValue, Double.MaxValue);
    private Point _commandPoint = new (0,0,0, Double.MaxValue, Double.MaxValue);
    private MaxRateOfChange _mrcAngle = new ();
    private bool _pathAngleCalculationActivated = false;
    private bool _commandInRange;

    public DroneOversight(ILogger<Worker> logger, double maxX, double maxY, int id, 
        List<AckableCoordinate> predefinedPathPoints, List<DroneHerder> herders) :
        base(maxX, maxY, id)
    {
        _logger = logger;
        _predefinedPathPoints = predefinedPathPoints;
        _herders = herders;
        
        _machine.Configure(State.FetchingFirstHerd)
            .PermitIf(Trigger.NewHerdCollected, State.FollowPath);
        
        _machine.Configure(State.FetchingNewHerd)
            .Permit(Trigger.NewHerdCollected, State.FollowPath);

        _machine.Configure(State.FollowPath)
            .Permit(Trigger.NewHerdInRange, State.FetchingNewHerd)
            .Permit(Trigger.SheepEscaped, State.RecollectSheep)
            .Permit(Trigger.AllSheepsAtFinish, State.Finished);

        _machine.Configure(State.RecollectSheep)
            .Permit(Trigger.SheepCaptured, State.FollowPath);
    }

    public (int pathIndex, List<Coordinate> centroids, Coordinate current, Coordinate next, string state, IList<Point> points) UpdatePosition(bool disableHerders, double dt, double[] settings,
        List<Sheep> sheeps)
    {
        // _herdRadius = settings[0]; //largestDistance; // settings[0];
        _herdAngleInRadians = settings[1] == 0 ? _herdAngleInRadians : settings[1];
        _speed = settings[2];
        var force = new Vector2(0, 0);

        
        // Clustering different herds
        var groups = Clustering.Cluster(sheeps.Where(s => !s.IsInsideFinishZone()).ToList<Point>(), 100.0);
        var centroids = groups
            .Select(g => 
                Calculator.Centroid(g.Select(gg => new Coordinate(gg.Position.X, gg.Position.Y)).ToList()))
            .OrderBy(g => Converter.ToVector2(Position, g).Length())
            .ToList();
        
        // Move the drone
        var closestPathPoint = _predefinedPathPoints
            .Where(p => (!p.Accessed && p.PathIndex >= _pathIndex) || (p.Accessed && p.PathIndex > _pathIndex))
            .OrderBy(p => Converter.ToVector2(p, centroids[0]).Length())
            .ToList();
        _pathIndex = closestPathPoint == null || closestPathPoint.Count < 1 ? _predefinedPathPoints.Count - 1 : closestPathPoint[0].PathIndex;
    
        _commandInRange = Calculator.UnderWithHysteresis(_commandInRange, Position, _command, 50.0, 10.0);
        var currentInRange = Calculator.InRange(Position, _current, 25.0, 0.0);
        var sheepCentroidInRange = Calculator.InRange(Position, centroids[0], 25.0, 0.0);
        
        if (_machine.State == State.FetchingFirstHerd && currentInRange)
        {
            _machine.Fire(Trigger.NewHerdCollected);
        }
        
        if (currentInRange && sheepCentroidInRange && _current is AckableCoordinate ack)
        {
            ack.Ack();
        }

        var lenghtToNextHerd =
            centroids.Count < 2  ? int.MaxValue : Converter.ToVector2(Position, centroids[1]).Length();
        var lenghtToNextPathPoint = Converter.ToVector2(Position, closestPathPoint[0]).Length();
        
        if (_machine.State == State.FollowPath && lenghtToNextHerd < lenghtToNextPathPoint)
        {
            _machine.Fire(Trigger.NewHerdInRange);
        }
        
        if (_machine.State == State.FetchingNewHerd && centroids.Count < 2)
        {
            _machine.Fire(Trigger.NewHerdCollected);
        }


        switch (_machine.State)
        {
            case State.FetchingFirstHerd:
                _current = centroids[0];
                _next = closestPathPoint[0];
                var nextVector = Converter.ToVector2(_current, _next);
                _nextPoint = new Point(0, 0, 0, _current.X, _current.Y);
                _nextPoint.Force = nextVector;
                var adjustedNextVector = Vector2.Multiply(Vector2.Normalize(Calculator.RotateVector(nextVector, Math.PI)), 150.0f);
                _commandPoint = new Point(0, 0, 0, _current.X + adjustedNextVector.X, _current.Y + adjustedNextVector.Y);
                _commandPoint.Force = Vector2.Negate(adjustedNextVector);
                _command =  _commandInRange ? _current : _commandPoint.Position; // TODO denne slåes av og på. Implementer inner state - gå mot commando så mot current
                break;
            case State.FetchingNewHerd:
                _current = centroids[1];
                _command = _current;
                break;
            case State.FollowPath:
                _current = closestPathPoint[0];
                _command = _current;
                break;
            case State.RecollectSheep:
                break;
            case State.Finished:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        /**
        // Calculate the angle the herders should have when closing in on a path point
        var pathAngle = 0.0;
        if (pathVector.Length() < 100) _pathAngleCalculationActivated = true;
        if (pathVector.Length() > 150) _pathAngleCalculationActivated = false;
        if (_pathAngleCalculationActivated)
        {
            var sheepVpathNext = Converter.ToVector2(
                _current,
                _next);
            
            pathAngle = Calculator.AngleInRadiansLimited(pathVector, sheepVpathNext);
            _logger.LogInformation(
                $"{nameof(pathAngle)}: {pathAngle}" +
                $"{nameof(pathVector)}: {pathVector}" +
                $"{nameof(sheepVpathNext)}: {sheepVpathNext}"
            );
        }
        **/

        
        
        
        var pathVector = Converter.ToVector2(Position, new Coordinate(_command.X, _command.Y));
        var pathPoint = new Point(0, 0, 0, Position.X, Position.Y);
        pathPoint.Force = pathVector;
        
        
        force = Vector2.Add(force, Vector2.Multiply(Vector2.Normalize(pathVector), (float) _speed));
        Force = Vector2.Multiply(force, 10); // For visualization purposes only
        Position.Update(Position.X + (force.X * (dt / 100)), Position.Y + (force.Y * (dt / 100)));

        // After updating this position, update the herders
        var h0 = Vector2.Multiply( Vector2.Negate(Vector2.Normalize(pathVector)), (float) _herdRadius);
        var h1 = Calculator.RotateVector(h0, _herdAngleInRadians);
        var h2 = Calculator.RotateVector(h0, -_herdAngleInRadians);
        
        if(disableHerders) return (_pathIndex, centroids.ToList(), _command, _next, _machine.State.ToString(), new List<Point>());
        _herders[0].UpdatePosition(dt, new Coordinate(Position.X + h0.X, Position.Y + h0.Y));
        _herders[1].UpdatePosition(dt, new Coordinate(Position.X + h1.X, Position.Y + h1.Y));
        _herders[2].UpdatePosition(dt, new Coordinate(Position.X + h2.X, Position.Y + h2.Y));
        return (_pathIndex, centroids.ToList(), _command, _next, _machine.State.ToString(), new List<Point>{pathPoint, _nextPoint, _commandPoint});
    }

    public double GetHerdingCircleRadius()
    {
        return _herdRadius;
    }
}