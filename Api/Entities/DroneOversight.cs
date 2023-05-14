using System.Numerics;
using SheepHerding.Api.Helpers;
using SheepHerding.Api.StateMachine;

namespace SheepHerding.Api.Entities;

public class DroneOversight : Point
{
    private readonly Point _commandPoint = new(0, double.MaxValue, double.MaxValue);
    private readonly double _herdAngleInRadians = Math.PI / 2.5;
    private readonly List<DroneHerder> _herders;
    private readonly ILogger _logger;
    private readonly Machine _machine;
    private readonly PathCoordinator _pathCoordinator;
    private readonly int _pathIndex = 0; // TODO fix this

    private readonly PointCreator _pointCreator;
    private readonly List<Sheep> _sheeps;

    private List<(Coordinate Position, double Radius)> _centroids;
    private AckableCoordinate _command = new(-1, 0, 0);
    private bool _commandInRange;
    private IList<AckableCoordinate> _commands = new List<AckableCoordinate>();
    private Coordinate _current = new(double.MaxValue, double.MaxValue);
    private bool _herdInPosesion;
    private double _herdRadius = 75.0;
    private Coordinate _next = new(double.MaxValue, double.MaxValue);
    private Point _nextPoint = new(0, double.MaxValue, double.MaxValue);
    private bool _pathAngleCalculationActivated = false;
    private Vector2 _positionCommandVector = Vector2.One;
    private double _speedFactor;


    public DroneOversight(ILogger logger, double maxX, double maxY, int id,
        PathCoordinator pathCoordinator, List<DroneHerder> herders, PointCreator pointCreator, List<Sheep> sheeps,
        Coordinate finish) :
        base(id)
    {
        _logger = logger;
        _pathCoordinator = pathCoordinator;
        _herders = herders;
        _pointCreator = pointCreator;
        _sheeps = sheeps;
        _machine = new Machine(_logger);
        InitializeStateMachine();
        StartupCalculations();
    }

    private void InitializeStateMachine()
    {
        _machine.ExecuteOnEntry(State.FetchingFirstHerd, () =>
        {
            // _logger.LogInformation("On entry FetchingFirstHerd");
            _centroids = GetSheepHerdCentroids(_sheeps);
            _current = _centroids[0].Position;
            _pathCoordinator.UpdateToClosest(_centroids[0].Position);
            _next = _pathCoordinator.GetCurrent(PATH_EXECUTER.HERDER);
            _commands = _pointCreator.CurvedLineToFetchHerd(Position, _current, _next);
        });
        _machine.ExecuteOnEntry(State.FollowPath, () =>
        {
            // _logger.LogInformation("On entry FollowPath");
            _current = _pathCoordinator.GetCurrent(PATH_EXECUTER.HERDER);
            _next = _pathCoordinator.GetNext(PATH_EXECUTER.HERDER);
            _commands = _pointCreator.CurvedLineToFollowPath(Position, _current, _next);
        });
        _machine.ExecuteOnEntry(State.AckPathCoordinator, () =>
        {
            // _logger.LogInformation("On entry Waiting");
            _pathCoordinator.Ack(PATH_EXECUTER.HERDER);
        });
    }

    private void StartupCalculations()
    {
        _centroids = GetSheepHerdCentroids(_sheeps);
    }


    private List<(Coordinate Position, double Radius)> GetSheepHerdCentroids(List<Sheep> sheeps)
    {
        if (!sheeps.Any()) return new List<(Coordinate Position, double Radius)>();
        var notFinishedSheeps = sheeps.Where(s => !s.IsInsideFinishZone()).ToList<Point>();
        if (!notFinishedSheeps.Any()) return new List<(Coordinate Position, double Radius)>();
        var groups =
            Clustering.Cluster(notFinishedSheeps, 100.0); // TODO denne limiten bør være i settings og skrive om
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

    public (
        int pathIndex,
        List<Coordinate> centroids,
        Coordinate current,
        Coordinate next,
        string state,
        IList<Coordinate> points,
        Point dummy) UpdatePosition(bool disableHerders, bool interceptCross)
    {
        // -- Calculations
        if (!_sheeps.Any())
            return (_pathIndex, _centroids.Select(c => c.Position).ToList(), _command, _next, _machine.State.ToString(),
                new List<Coordinate>(), new Point(-100));
        // Clustering different herds
        var newCentroids = GetSheepHerdCentroids(_sheeps);
        _centroids = newCentroids.Count > 0 ? newCentroids : _centroids;

        // Move the drone

        // -- Setting states
        _commandInRange = Calculator.UnderWithHysteresis(_commandInRange, Position, _command, 5.0, 1.0);
        var currentInRange = Calculator.InRange(Position, _current, 45.0, 0.0);
        var closestPathPointOutOfRange = Calculator.InRange(Position,
            _pathCoordinator.GetCurrent(PATH_EXECUTER.HERDER), 10000.0,
            10.0);


        _machine.Fire(State.FetchingFirstHerd, Trigger.NewHerdCollected, () => currentInRange);
        _machine.Fire(State.FollowPath, Trigger.CommandsExecuted, () => _commands.All(c => c.Accessed));
        _machine.Fire(State.AckPathCoordinator, Trigger.PathPointOutOfRange, () => closestPathPointOutOfRange);
        _machine.Fire(State.Start, Trigger.Start, () => true);
        _machine.Fire(Trigger.AllSheepsAtFinish, () => _sheeps.All(s => s.IsInsideFinishZone()));
        _machine.Fire(State.FollowPathStraight, Trigger.IntersectionApproaching,
            () => _pathCoordinator.IntersectionApproaching(Position) && interceptCross);

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
        var force = Vector2.Divide(Vector2.Normalize(_positionCommandVector), 2.0f);
        Force = Vector2.Multiply(force, 10); // For visualization purposes only
        Position.Update(Position.X + force.X, Position.Y + force.Y);
        // _logger.LogInformation($"Speed O: {force.Length()}");


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

        var rotatAdjustmentForIntersection = Math.PI / 5.0;

        // Calculate Vectors to next position for herders
        // Controlling the herd radius relative to how much spreading there is between the sheeps
        _herdRadius = Math.Pow(_centroids[0].Radius, 1.2);
        if (_herdRadius < 90.0) _herdRadius = 90.0;
        if (_herdRadius > 110.0) _herdRadius = 110.0;

        var h0 = Vector2.Multiply(Vector2.Negate(Vector2.Normalize(_positionCommandVector)), (float) _herdRadius);
        h0 = Calculator.RotateVector(h0,
            angleAdjustmentForOutcastSheep + (_machine.State == State.FollowPathIntersectionLeft
                ? rotatAdjustmentForIntersection
                : 0.0));

        var h1 = Calculator.RotateVector(h0, _herdAngleInRadians);

        if (_machine.State == State.FollowPathIntersectionLeft)
        {
            // Send drone to block
            var nextCross = _pathCoordinator.GetNextCross();
            if (nextCross != null)
            {
                var nextSheepCoordinateAfterCross = nextCross.GetNext(PATH_EXECUTER.SHEEP).ThisCoordinate;
                var cross = nextCross.ThisCoordinate;
                var vector = Vector2.Multiply(
                    Vector2.Normalize(
                        Converter.ToVector2(
                            cross,
                            nextSheepCoordinateAfterCross)
                    ),
                    80.0f);
                h1 = vector;
            }
        }

        var h2 = Calculator.RotateVector(h0, -_herdAngleInRadians);

        if (disableHerders)
            return (_pathIndex, _centroids.Select(c => c.Position).ToList(), _command, _next, _machine.State.ToString(),
                new List<Coordinate>(), new Point(-100));

        // Commands the herders to move to next coordinate
        _herders[0].UpdatePosition(new Coordinate(Position.X + h0.X, Position.Y + h0.Y));
        _herders[1].UpdatePosition(new Coordinate(Position.X + h1.X, Position.Y + h1.Y));
        _herders[2].UpdatePosition(new Coordinate(Position.X + h2.X, Position.Y + h2.Y));
        var pointList = new List<Coordinate> {pathPoint.Position, _nextPoint.Position, _commandPoint.Position};
        pointList.AddRange(_commands);
        return (_pathIndex, _centroids.Select(c => c.Position).ToList(), _command, _next, _machine.State.ToString(),
            pointList, angleToCenterDummy);
    }

    public double GetHerdingCircleRadius()
    {
        return _herdRadius;
    }
}