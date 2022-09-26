using System.Numerics;
using SheepHerding.Api.Helpers;
using SheepHerding.Api.Services;
using SheepHerding.Api.StateMachine;
using Stateless;

namespace SheepHerding.Api.Entities;

public class DroneOversight : Point
{
    private readonly StateMachine<State,Trigger> _machine = new (State.FetchingFirstHerd);

    private readonly ILogger _logger;
    private readonly List<AckableCoordinate> _predefinedPathPoints;
    private readonly List<DroneHerder> _herders;
    private double _speed = 10.0;
    private double _herdRadius = 75.0;
    private double _herdAngleInRadians = Math.PI / 2.5;
    private int _pathIndex = 0; // TODO fix this
    private bool _herdInPosesion;
    private Coordinate _current = new (Double.MaxValue, Double.MaxValue);
    private Coordinate _next = new (Double.MaxValue, Double.MaxValue);
    private Coordinate _command = new (Double.MaxValue, Double.MaxValue);
    private IList<Coordinate> _commands = new List<Coordinate>();
    private Point _nextPoint = new (0,0, 0, Double.MaxValue, Double.MaxValue);
    private Point _commandPoint = new (0,0,0, Double.MaxValue, Double.MaxValue);
    private MaxRateOfChange _mrcAngle = new ();
    private bool _pathAngleCalculationActivated = false;
    private bool _commandInRange;
    private Vector2 _positionCommandVector;

    public DroneOversight(ILogger logger, double maxX, double maxY, int id, 
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

    public (int pathIndex, List<Coordinate> centroids, Coordinate current, Coordinate next, string state, IList<Coordinate> points) UpdatePosition(bool disableHerders, double dt, double[] settings,
        List<Sheep> sheeps)
    {
        // _herdRadius = settings[0]; //largestDistance; // settings[0];
        _herdAngleInRadians = settings[1] == 0 ? _herdAngleInRadians : settings[1];
        _speed = settings[2];
        var force = new Vector2(0, 0);

        // -- Calculations
        
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
    
        // -- Setting states
        _commandInRange = Calculator.UnderWithHysteresis(_commandInRange, Position, _command, 50.0, 10.0);
        var currentInRange = Calculator.InRange(Position, _current, 45.0, 0.0);
        var sheepCentroidInRange = Calculator.InRange(Position, centroids[0], 45.0, 0.0);
        var sheepCentroidOutOfRange = Calculator.InRange(Position, centroids[0], 100.0, 50.0);
        
        if (_machine.State == State.FetchingFirstHerd && currentInRange)
        {
            _machine.Fire(Trigger.NewHerdCollected);
        }
        
        if (currentInRange && sheepCentroidInRange && _current is AckableCoordinate ack)
        {
            ack.Ack();
        }

        if (_machine.State == State.FollowPath && sheepCentroidOutOfRange)
        {
            _machine.Fire(Trigger.SheepEscaped);
        }
        
        if (_machine.State == State.RecollectSheep && sheepCentroidInRange)
        {
            _machine.Fire(Trigger.SheepCaptured);
        }

        var lenghtToNextHerd = centroids.Count < 2  ? int.MaxValue : Converter.ToVector2(Position, centroids[1]).Length();
        var lenghtToNextPathPoint = Converter.ToVector2(Position, closestPathPoint[0]).Length();
        
        var lenghtFromCurrentToHerd = centroids.Count < 2  ? int.MaxValue : Converter.ToVector2(_current, centroids[1]).Length();
        var lenghtFromNextToHerd = centroids.Count < 2  ? int.MaxValue : Converter.ToVector2(_next, centroids[1]).Length();
        
        if (_machine.State == State.FollowPath && lenghtFromCurrentToHerd < lenghtFromNextToHerd)
        {
            _machine.Fire(Trigger.NewHerdInRange);
        }
        
        if (_machine.State == State.FetchingNewHerd && currentInRange)
        {
            _machine.Fire(Trigger.NewHerdCollected);
        }
        
        

        // -- Acting according to state
        switch (_machine.State)
        {
            case State.FetchingFirstHerd:
                _current = centroids[0];
                _next = closestPathPoint[0];
                _commandPoint = GetCommandPointV3(_command, _current, _next, 100.0);
                _command =  _current;
                break;
            case State.FetchingNewHerd:
                _current = centroids.Count > 1 ? centroids[1] : centroids[0];
                _next = closestPathPoint[0];
                _commandPoint = GetCommandPointV3(_command, _current, _next, 100.0);
                _command = _current; //centroids.Count > 1 ? _commandPoint.Position : _current;
                break;
            case State.FollowPath:
                _current = closestPathPoint[0];
                _next = closestPathPoint.Count > 1 ? closestPathPoint[1] : closestPathPoint[0];
                _commandPoint = GetCommandPointV3(_command, _current, _next, 150.0);
                _command = _current; //_commandPoint.Position;
                break;
            case State.RecollectSheep:
                _current = centroids[0];
                _next = closestPathPoint[0];
                _command = _current;
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

        
        // Write out
        _nextPoint = new Point(0, 0, 0, _current.X, _current.Y);
        _positionCommandVector = Converter.ToVector2(Position, new Coordinate(_command.X, _command.Y));
        var pathPoint = new Point(0, 0, 0, Position.X, Position.Y);
        pathPoint.Force = _positionCommandVector;

        
        force = Vector2.Add(force, Vector2.Multiply(Vector2.Normalize(_positionCommandVector), 4.0f));
        Force = Vector2.Multiply(force, 10); // For visualization purposes only
        Position.Update(Position.X + (force.X * (dt / 100)), Position.Y + (force.Y * (dt / 100)));

        // After updating this position, update the herders
        var h0 = Vector2.Multiply( Vector2.Negate(Vector2.Normalize(_positionCommandVector)), (float) _herdRadius);
        var h1 = Calculator.RotateVector(h0, _herdAngleInRadians);
        var h2 = Calculator.RotateVector(h0, -_herdAngleInRadians);
        
        if(disableHerders) return (_pathIndex, centroids.ToList(), _command, _next, _machine.State.ToString(), new List<Coordinate>());
        _herders[0].UpdatePosition(dt, new Coordinate(Position.X + h0.X, Position.Y + h0.Y));
        _herders[1].UpdatePosition(dt, new Coordinate(Position.X + h1.X, Position.Y + h1.Y));
        _herders[2].UpdatePosition(dt, new Coordinate(Position.X + h2.X, Position.Y + h2.Y));
        var pointList = new List<Coordinate> {pathPoint.Position, _nextPoint.Position, _commandPoint.Position};
        pointList.AddRange(_commands);
        return (_pathIndex, centroids.ToList(), _command, _next, _machine.State.ToString(), pointList);
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
        var rotatedNormNextVector = Vector2.Normalize(Calculator.RotateVector(nextVector, Math.PI/2));
        var adjustedNextVector = Vector2.Multiply(Vector2.Normalize(rotatedNormNextVector), (float)reduction);
        
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
        if (positionCurrentVector.Length()*2 > reductionStart && pathVector.Length()*2 > reductionStart)
        {
            reduction = reductionStart;
        } 
        else if (positionCurrentVector.Length() > pathVector.Length())
        {
            reduction = pathVector.Length()*2;
        }
        else
        {
            reduction = positionCurrentVector.Length()*2;
        }
        var rotatedNormNextVector = Vector2.Normalize(Calculator.RotateVector(currentNextVector, angles));
        var adjustedNextVector = Vector2.Multiply(Vector2.Normalize(rotatedNormNextVector), (float)reduction);
        
        var command = new Point(0, 0, 0, current.X + adjustedNextVector.X, current.Y + adjustedNextVector.Y);
        command.Force = Vector2.Negate(adjustedNextVector);
        return command;
    }
    
    private Point GetCommandPointV3(Coordinate previousCommand, Coordinate current, Coordinate next, double reductionStart)
    {
        var commandVector = Vector2.Multiply(Calculator.GetCommandVector(Position, current, next), 100.0f);
        
        var command = new Point(0, 0, 0, current.X + commandVector.X, current.Y + commandVector.Y);
        command.Force = Vector2.Negate(commandVector);
        return command;
    }
}