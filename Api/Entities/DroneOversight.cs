using System.Numerics;
using SheepHerding.Api.Helpers;
using SheepHerding.Api.StateMachine;

namespace SheepHerding.Api.Entities;

public class DroneOversight : Point
{
    private readonly Point _commandPoint = new(0, double.MaxValue, double.MaxValue);
    private readonly Coordinate _finish;
    private readonly double _herdAngleInRadians = Math.PI / 2.5; //3.1 - For when sheep movement are adjusted
    private readonly List<DroneHerder> _herders;
    private readonly ILogger _logger;
    private readonly Machine _machine;

    // private List<AckableCoordinate> _closestPathPoints;
    // private List<AckableCoordinate> _closestPathPointToSheepCentroid;
    private readonly PathCreator _pathCreator;
    private readonly int _pathIndex = 0; // TODO fix this
    private readonly PathCoordinator _pathCoordinator;
    private readonly List<Sheep> _sheeps;

    private List<(Coordinate Position, double Radius)> _centroids;
    private AckableCoordinate _command = new(-1, 0, 0);
    private bool _commandInRange;
    private IList<AckableCoordinate> _commands = new List<AckableCoordinate>();
    private Coordinate _current = new(double.MaxValue, double.MaxValue);
    private bool _herdInPosesion;
    private double _herdRadius = 75.0;
    private MaxRateOfChange _mrcAngle = new();
    private Coordinate _next = new(double.MaxValue, double.MaxValue);
    private Point _nextPoint = new(0, double.MaxValue, double.MaxValue);
    private bool _pathAngleCalculationActivated = false;
    private Vector2 _positionCommandVector = Vector2.One;
    private double _speedFactor;


    public DroneOversight(ILogger logger, double maxX, double maxY, int id,
        PathCoordinator pathCoordinator, List<DroneHerder> herders, PathCreator pathCreator, List<Sheep> sheeps,
        Coordinate finish) :
        base(id)
    {
        _logger = logger;
        _pathCoordinator = pathCoordinator;
        _herders = herders;
        _pathCreator = pathCreator;
        _sheeps = sheeps;
        _finish = finish;
        _machine = new Machine(_logger);
        InitializeStateMachine();
        StartupCalculations();
    }

    private void InitializeStateMachine()
    {
        _machine.ExecuteOnEntry(State.FetchingFirstHerd, () =>
        {
            _logger.LogInformation("On entry FetchingFirstHerd");
            _centroids = GetSheepHerdCentroids(_sheeps);
            _current = _centroids[0].Position;
            // _closestPathPointToSheepCentroid = GetClosesPathPointToClosestSheepCentroid();
            _pathCoordinator.UpdateToClosest(_centroids[0].Position);
            _next = _pathCoordinator.GetCurrent(PATH_EXECUTER.HERDER); // _closestPathPointToSheepCentroid[0];
            // _commandPoint = GetCommandPointV3(_command, _current, _next, 100.0);
            // _command =  _current;
            _commands = _pathCreator.CurvedLineToFetchHerd(Position, _current, _next);
        });

        _machine.ExecuteOnEntry(State.Waiting, () =>
        {
            _logger.LogInformation("On entry Waiting");
            // if (_current is AckableCoordinate ack)
            // {
            //     // _logger.LogInformation("On entry Waiting ack");
            //     ack.Ack();
            // }
            _pathCoordinator.Ack(PATH_EXECUTER.HERDER);

            // _closestPathPoints = GetClosesPathPoint();
            // _current = _closestPathPoint[0];
        });

        // _machine.ExecuteOnEntry(State.FetchingNewHerd, () =>
        // {
        //     // _logger.LogInformation("On entry FetchingNewHerd");
        //     _centroids = GetSheepHerdCentroids(_sheeps);
        //     _current = _centroids.Count > 1 ? _centroids[1].Position : _centroids[0].Position;
        //     // _closestPathPointToSheepCentroid = GetClosesPathPointToClosestSheepCentroid();
        //     _predefinedPathPoints.UpdateToClosest(_centroids[0].Position);
        //     _next = _predefinedPathPoints.GetCurrent(PATH_EXECUTER.HERDER); // _closestPathPointToSheepCentroid[0];
        //     // _commandPoint = GetCommandPointV3(_command, _current, _next, 100.0);
        //     // _command = _current; //centroids.Count > 1 ? _commandPoint.Position : _current;
        // });
        _machine.ExecuteOnEntry(State.FollowPath, () =>
        {
            _logger.LogInformation("On entry FollowPath");
            // _closestPathPoints = GetClosesPathPoint();
            _current = _pathCoordinator.GetCurrent(PATH_EXECUTER.HERDER); // _closestPathPoints[0];
            _next = _pathCoordinator.GetNext(PATH_EXECUTER.HERDER);
            // _closestPathPoints.Count > 1 ? _closestPathPoints[1] : _closestPathPoints[0];
            // _commandPoint = GetCommandPointV3(_command, _current, _next, 150.0);
            // _command = _current; //_commandPoint.Position;
            _commands = _pathCreator.CurvedLineToFollowPath(Position, _current, _next);
        });
        // _machine.ExecuteOnEntry(State.RecollectSheep, () =>
        // {
        //     // _logger.LogInformation("On entry RecollectSheep");
        //     _centroids = GetSheepHerdCentroids(_sheeps);
        //     _current = _centroids[0].Position;
        //     // _closestPathPointToSheepCentroid = GetClosesPathPointToClosestSheepCentroid();
        //     _predefinedPathPoints.UpdateToClosest(_centroids[0].Position);
        //     _next = _predefinedPathPoints.GetCurrent(PATH_EXECUTER.HERDER); //  _closestPathPointToSheepCentroid[0];
        //     // _command = _current;
        // });
        _machine.ExecuteOnEntry(State.Finished, () => { _logger.LogInformation("On entry Finished"); });
    }

    private void StartupCalculations()
    {
        _centroids = GetSheepHerdCentroids(_sheeps);
        // _closestPathPoints = GetClosesPathPoint();
        // _closestPathPointToSheepCentroid = GetClosesPathPointToClosestSheepCentroid();
    }

    // private List<AckableCoordinate> GetClosesPathPoint()
    // {
    //     var points = _predefinedPathPoints
    //         .Where(p => (!p.Accessed && p.PathIndex >= _pathIndex) || (p.Accessed && p.PathIndex > _pathIndex))
    //         .OrderBy(p => p.PathIndex)
    //         .ToList();
    //     _pathIndex = points == null || points.Count < 1
    //         ? _predefinedPathPoints.Count - 1
    //         : points[0].PathIndex;
    //     return points;
    // }
    //
    // private List<AckableCoordinate> GetClosesPathPointToClosestSheepCentroid()
    // {
    //     if (!_centroids.Any()) return new List<AckableCoordinate>();
    //     var points = _predefinedPathPoints
    //         .Where(p => (!p.Accessed && p.PathIndex >= _pathIndex) || (p.Accessed && p.PathIndex > _pathIndex))
    //         .OrderBy(p => Converter.ToVector2(p, _centroids[0].Position).Length())
    //         .ToList();
    //     _pathIndex = points == null || points.Count < 1
    //         ? _predefinedPathPoints.Count - 1
    //         : points[0].PathIndex;
    //     return points;
    // }

    private List<(Coordinate Position, double Radius)> GetSheepHerdCentroids(List<Sheep> sheeps)
    {
        if (!sheeps.Any()) return new List<(Coordinate Position, double Radius)>();
        var notFinishedSheeps = sheeps.Where(s => !s.IsInsideFinishZone()).ToList<Point>();
        if (!notFinishedSheeps.Any()) return new List<(Coordinate Position, double Radius)>();
        var groups = Clustering.Cluster(notFinishedSheeps, 100.0);
        return groups
            .Select(g =>
            {
                var centroid =
                    Calculator.Centroid(g.Select(gg => new Coordinate(gg.Position.X, gg.Position.Y)).ToList());
                var radius = g == null
                    ? 0.0
                    : g.Max(gg =>
                        (double) Converter.ToVector2(centroid, new Coordinate(gg.Position.X, gg.Position.Y)).Length());
                return (centroid, radius);
            })
            .OrderBy(g => (Converter.ToVector2(Position, g.centroid).Length(), g.radius))
            .ToList();
    }

    public (int pathIndex, List<Coordinate> centroids, Coordinate current, Coordinate next, string state,
        IList<Coordinate> points, Point dummy) UpdatePosition(bool disableHerders, double forceAdjustment)
    {
        // -- Calculations
        if (!_sheeps.Any())
            return (_pathIndex, _centroids.Select(c => c.Position).ToList(), _command, _next, _machine.State.ToString(),
                new List<Coordinate>(), new Point(-100));
        // Clustering different herds
        var newCentroids = GetSheepHerdCentroids(_sheeps);
        _centroids = newCentroids.Count > 0 ? newCentroids : _centroids;

        // Move the drone

        // _closestPathPointToSheepCentroid = GetClosesPathPointToClosestSheepCentroid();

        // -- Setting states
        _commandInRange = Calculator.UnderWithHysteresis(_commandInRange, Position, _command, 5.0, 1.0);
        var currentInRange = Calculator.InRange(Position, _current, 45.0, 0.0);
        var sheepCentroidInRange = Calculator.InRange(Position, _centroids[0].Position, 45.0, 0.0);
        var sheepCentroidOutOfRange = Calculator.InRange(Position, _centroids[0].Position, 100.0, 50.0);
        var closestPathPointOutOfRange = Calculator.InRange(Position,
            _pathCoordinator.GetCurrent(PATH_EXECUTER.HERDER), 10000.0,
            10.0); //  _closestPathPoints.Count > 0 && Calculator.InRange(Position, _closestPathPoints[0], 10000.0, 10.0);


        var lenghtToNextHerd =
            _centroids.Count < 2 ? int.MaxValue : Converter.ToVector2(Position, _centroids[1].Position).Length();
        // var lenghtToNextPathPoint = Converter.ToVector2(Position, _closestPathPoints[0]).Length();

        var lenghtFromCurrentToHerd =
            _centroids.Count < 2 ? int.MaxValue : Converter.ToVector2(_current, _centroids[1].Position).Length();
        var lenghtFromNextToHerd =
            _centroids.Count < 2 ? int.MaxValue : Converter.ToVector2(_next, _centroids[1].Position).Length();

        _machine.Fire(State.FetchingFirstHerd, Trigger.NewHerdCollected, () => currentInRange);
        // _machine.Fire(State.FetchingNewHerd, Trigger.NewHerdCollected, () => currentInRange);
        // _machine.Fire(State.FollowPath, Trigger.SheepEscaped, () => sheepCentroidOutOfRange);
        // _machine.Fire(State.FollowPath, Trigger.NewHerdInRange, () => lenghtFromCurrentToHerd < lenghtFromNextToHerd);
        _machine.Fire(State.FollowPath, Trigger.CommandsExecuted, () => _commands.All(c => c.Accessed));
        _machine.Fire(State.Waiting, Trigger.PathPointOutOfRange, () => closestPathPointOutOfRange);
        // _machine.Fire(State.RecollectSheep, Trigger.SheepCaptured, () => sheepCentroidInRange);
        _machine.Fire(State.Start, Trigger.Start, () => true);
        _machine.Fire(Trigger.AllSheepsAtFinish, () => _sheeps.All(s => s.IsInsideFinishZone()));
        _machine.Fire(State.FollowPathStraight, Trigger.CornerApproaching, () => _command.IsPartOfCurve);
        // TODO
        _machine.Fire(State.FollowPathStraight, Trigger.IntersectionApproaching,
            () => _pathCoordinator.IntersectionApproaching(Position));
        _machine.Fire(State.FollowPathCorner, Trigger.IntersectionApproaching,
            () => _pathCoordinator.IntersectionApproaching(Position));

        // Write out
        if (_commandInRange) _command.Ack();

        _nextPoint = new Point(0, _current.X, _current.Y);
        var c = _commands.FirstOrDefault(c => !c.Accessed);
        if (c != null)
        {
            _command = c;
            _positionCommandVector = Converter.ToVector2(Position, new Coordinate(_command.X, _command.Y));
        }

        var pathPoint = new Point(0, Position.X, Position.Y); // For visualization only
        pathPoint.Force = _positionCommandVector; // For visualization only


        // Update drone oversight position
        var force = Vector2.Multiply(Vector2.Normalize(_positionCommandVector), 4.0f);
        Force = Vector2.Multiply(force, 10); // For visualization purposes only
        Position.Update(Position.X + force.X * forceAdjustment, Position.Y + force.Y * forceAdjustment);

        // Controlling the herd radius relative to how much spreading there is between the sheeps
        _herdRadius = Math.Pow(_centroids[0].Radius, 1.2);
        if (_herdRadius < 90.0) _herdRadius = 90.0;
        if (_herdRadius > 110.0) _herdRadius = 110.0;

        // Adjustment for sheep herd out of center
        var outcast = Calculator.Outcast(_sheeps.Select(s => s.Position).ToList(), _centroids[0].Position);
        var oversightSheepOutcastVector = Converter.ToVector2(Position, outcast);
        var angleToCenterDummy = new Point(-1, Position.X, Position.Y);
        angleToCenterDummy.Force = oversightSheepOutcastVector;

        var angleAdjustmentForOutcastSheep = 0.0;
        if (_machine.State == State.FollowPathStraight)
        {
            angleAdjustmentForOutcastSheep =
                Calculator.AngleInRadiansLimited(oversightSheepOutcastVector, _positionCommandVector);
            if (oversightSheepOutcastVector.Length() < 25.0 || Math.Abs(angleAdjustmentForOutcastSheep) > Math.PI / 2)
                angleAdjustmentForOutcastSheep = 0.0;
        }

        // Calculate Vectors to next position for herders
        var h0 = Vector2.Multiply(Vector2.Negate(Vector2.Normalize(_positionCommandVector)), (float) _herdRadius);
        h0 = Calculator.RotateVector(h0, angleAdjustmentForOutcastSheep * 1.0);
        var h1 = Calculator.RotateVector(h0, _herdAngleInRadians);
        var h2 = Calculator.RotateVector(h0, -_herdAngleInRadians);

        if (disableHerders)
            return (_pathIndex, _centroids.Select(c => c.Position).ToList(), _command, _next, _machine.State.ToString(),
                new List<Coordinate>(), new Point(-100));

        // Commands the herders to move to next coordinate
        _herders[0].UpdatePosition(forceAdjustment, new Coordinate(Position.X + h0.X, Position.Y + h0.Y));
        _herders[1].UpdatePosition(forceAdjustment, new Coordinate(Position.X + h1.X, Position.Y + h1.Y));
        _herders[2].UpdatePosition(forceAdjustment, new Coordinate(Position.X + h2.X, Position.Y + h2.Y));
        var pointList = new List<Coordinate> {pathPoint.Position, _nextPoint.Position, _commandPoint.Position};
        pointList.AddRange(_commands);
        return (_pathIndex, _centroids.Select(c => c.Position).ToList(), _command, _next, _machine.State.ToString(),
            pointList, angleToCenterDummy);
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
            reduction = reductionStart;
        else if (currentVector.Length() > pathVector.Length())
            reduction = pathVector.Length();
        else
            reduction = currentVector.Length();

        var rotatedNormNextVector = Vector2.Normalize(Calculator.RotateVector(nextVector, Math.PI / 2));
        var adjustedNextVector = Vector2.Multiply(Vector2.Normalize(rotatedNormNextVector), (float) reduction);

        var command = new Point(0, current.X + adjustedNextVector.X, current.Y + adjustedNextVector.Y);
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
            reduction = reductionStart;
        else if (positionCurrentVector.Length() > pathVector.Length())
            reduction = pathVector.Length() * 2;
        else
            reduction = positionCurrentVector.Length() * 2;

        var rotatedNormNextVector = Vector2.Normalize(Calculator.RotateVector(currentNextVector, angles));
        var adjustedNextVector = Vector2.Multiply(Vector2.Normalize(rotatedNormNextVector), (float) reduction);

        var command = new Point(0, current.X + adjustedNextVector.X, current.Y + adjustedNextVector.Y);
        command.Force = Vector2.Negate(adjustedNextVector);
        return command;
    }

    private Point GetCommandPointV3(Coordinate previousCommand, Coordinate current, Coordinate next,
        double reductionStart)
    {
        var commandVector = Vector2.Multiply(Calculator.GetCommandVector(Position, current, next), 100.0f);

        var command = new Point(0, current.X + commandVector.X, current.Y + commandVector.Y);
        command.Force = Vector2.Negate(commandVector);
        return command;
    }
}