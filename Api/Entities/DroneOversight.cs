using System.Numerics;
using SheepHerding.Api.Helpers;

namespace SheepHerding.Api.Entities;

public class DroneOversight : Point
{
    private readonly double _finishX;
    private readonly double _finishY;
    private double _speed = 10.0;
    private double _herdRadius = 75.0;
    internal Coordinate[] HerdCommands = new []{new Coordinate(0,0), new Coordinate(0,0), new Coordinate(0,0)};
    private double _herdAngle = Math.PI / 3;

    public DroneOversight(double maxX, double maxY, int id, double finishX, double finishY) : base(maxX, maxY, id)
    {
        _finishX = finishX;
        _finishY = finishY;
    }

    public override void UpdatePosition(Coordinate sheepCentroid, double dt, double[] settings)
    {
        _herdRadius = settings[0];
        _herdAngle = settings[1];
        _speed = settings[2];
        var force = new Vector2(0, 0);
        var sheepVcentroid = new Vector2(Convert.ToSingle(Position.X - sheepCentroid.X), Convert.ToSingle(Position.Y - sheepCentroid.Y));
        var sheepVcentroidReduced = Vector2.Divide(sheepVcentroid, 5);
        var negated = Vector2.Negate(sheepVcentroidReduced);
        force = Vector2.Add(force, negated);
        
        
        var sheepVfinish = new Vector2(Convert.ToSingle(Position.X - _finishX), Convert.ToSingle(Position.Y - _finishY));
        var sheepVfinishNorm = Vector2.Normalize(sheepVfinish);
        var sheepVfinishNormNegated = Vector2.Negate(sheepVfinishNorm);
        force = Vector2.Add(force, Vector2.Multiply(sheepVfinishNormNegated, (float)_speed));


        Force = Vector2.Multiply(force, 10); // For visualization purposes only
        Position.Update(Position.X + (force.X * (dt/100)), Position.Y + (force.Y * (dt/100)));
        
        // After updating this position, update the herders
        var h0 = Vector2.Multiply(sheepVfinishNorm, (float)_herdRadius);
        var h1 = Calculator.RotateVector(h0, _herdAngle);
        var h2 = Calculator.RotateVector(h0, -_herdAngle);
        
            
        HerdCommands[0] = new Coordinate(Position.X + h0.X, Position.Y + h0.Y);
        HerdCommands[1] = new Coordinate(Position.X + h1.X, Position.Y + h1.Y);
        HerdCommands[2] = new Coordinate(Position.X + h2.X, Position.Y + h2.Y);
    }

    public double GetHerdingCircleRadius()
    {
        return _herdRadius;
    }
}