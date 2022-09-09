using System.Numerics;
using SheepHerding.Api.Helpers;
using SheepHerding.Api.Workers;

namespace SheepHerding.Api.Entities;

public class DroneOversight : Point
{
    private readonly ILogger<Worker> _logger;
    private double _speed = 10.0;
    private double _herdRadius = 75.0;
    internal Coordinate[] HerdCommands = new[] {new Coordinate(0, 0), new Coordinate(0, 0), new Coordinate(0, 0)};
    private double _herdAngleInRadians = Math.PI / 3;
    private int _pathIndex = 1; // TODO fix this

    public DroneOversight(ILogger<Worker> logger, double maxX, double maxY, int id) :
        base(maxX, maxY, id)
    {
        _logger = logger;
    }

    public int UpdatePosition(Coordinate sheepCentroid, double largestDistance, double dt, double[] settings,
        List<Coordinate> path)
    {
        _herdRadius = settings[0]; //largestDistance; // settings[0];
        _herdAngleInRadians = settings[1] == 0 ? _herdAngleInRadians : settings[1];
        _speed = settings[2];
        var force = new Vector2(0, 0);
        var sheepVcentroid = Converter.ToVector2(Position, sheepCentroid); 
        var sheepVcentroidReduced = Vector2.Divide(sheepVcentroid, 5);
        force = Vector2.Add(force, sheepVcentroidReduced);

        // Path
        var sheepVpath = Converter.ToVector2(Position.X, Position.Y, _pathIndex < path.Count ? path[_pathIndex].X : MaxX, _pathIndex < path.Count ? path[_pathIndex].Y : MaxY);
        var sheepVpathNorm = Vector2.Normalize(sheepVpath);
        var sheepVpathNormNegated = Vector2.Negate(sheepVpathNorm);
        force = Vector2.Add(force, Vector2.Multiply(sheepVpathNorm, (float) _speed));
        
        if (sheepVpath.Length() < 50)
        {
            _pathIndex++;
        }
        
        var pathAngle = 0.0;
        if (sheepVpath.Length() < 100 && _pathIndex < path.Count - 1)
        {
            var sheepVpathNext = Converter.ToVector2(
                path[_pathIndex],
                path[_pathIndex + 1]);
            
            pathAngle = Calculator.AngleInRadiansLimited(sheepVpath, sheepVpathNext);
            _logger.LogInformation(
                $"{nameof(pathAngle)}: {pathAngle}" +
                $"{nameof(sheepVpath)}: {sheepVpath}" +
                $"{nameof(sheepVpathNext)}: {sheepVpathNext}"
            );
        }

        Force = Vector2.Multiply(force, 10); // For visualization purposes only
        Position.Update(Position.X + (force.X * (dt / 100)), Position.Y + (force.Y * (dt / 100)));

        // After updating this position, update the herders
        var sheepVpathNormNegatedRotated = Calculator.RotateVector(sheepVpathNormNegated, pathAngle/2.0);
        var h0 = Vector2.Multiply(sheepVpathNormNegatedRotated, (float) _herdRadius);
        // var h0 = Vector2.Multiply(sheepVfinishNorm, (float)_herdRadius);
        var h1 = Calculator.RotateVector(h0, _herdAngleInRadians);
        var h2 = Calculator.RotateVector(h0, -_herdAngleInRadians);
        
        
        HerdCommands[0] = new Coordinate(sheepCentroid.X + h0.X, sheepCentroid.Y + h0.Y);
        HerdCommands[1] = new Coordinate(sheepCentroid.X + h1.X, sheepCentroid.Y + h1.Y);
        HerdCommands[2] = new Coordinate(sheepCentroid.X + h2.X, sheepCentroid.Y + h2.Y);
        return _pathIndex;
    }

    public double GetHerdingCircleRadius()
    {
        return _herdRadius;
    }
}