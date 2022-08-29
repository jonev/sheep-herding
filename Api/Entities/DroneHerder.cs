using System.Numerics;
using SheepHerding.Api.Helpers;

namespace SheepHerding.Api.Entities;

public class DroneHerder : Point
{
    private readonly DroneOversight _oversight;
    private readonly IList<DroneHerder> _herders;
    private double _closeThreshold = 10.0;
    private double _oversightThreshold = 7.0;

    public DroneHerder(double maxX, double maxY, int id, DroneOversight oversight, IList<DroneHerder> herders) : base(maxX, maxY, id)
    {
        _oversight = oversight;
        _herders = herders;
    }

    public override void UpdatePosition(Coordinate sheepCentroid, double dt, double[] settings)
    {
        _closeThreshold = settings[0];
        _oversightThreshold = settings[1];
        
        var force = new Vector2(0, 0);
        
        var close = _herders.Where(s => s.Id != Id &&
                (Math.Abs(s.Position.X - Position.X) <= _closeThreshold) && (Math.Abs(s.Position.Y - Position.Y) <= _closeThreshold)).ToList();
        if (close.Any())
        {
            var list = close.Select(s => s.Position).ToList();
            list.Add(Position);
            var closeCentroid = Calculator.Centroid(list);
            var sheepVclose = new Vector2(Convert.ToSingle(Position.X - closeCentroid.x), 
                Convert.ToSingle(Position.Y - closeCentroid.y));
            force = Vector2.Add(force, sheepVclose);
        }
        
        
        // var sheepVcentroid = new Vector2(Convert.ToSingle(Position.X - sheepCentroid.X), Convert.ToSingle(Position.Y - sheepCentroid.Y));
        // var sheepVcentroidReduced = Vector2.Divide(sheepVcentroid, 10);
        
        
        var oversightVcentroid = new Vector2(Convert.ToSingle(Position.X - _oversight.Position.X), Convert.ToSingle(Position.Y - _oversight.Position.Y));
        var oversightVcentroidReduced = Vector2.Divide(oversightVcentroid, 10);
        var negated = Vector2.Negate(oversightVcentroidReduced);
        if (negated.Length() >= _oversightThreshold)
        {
            force = Vector2.Add(force, negated);
        }
        
        Force = Vector2.Multiply(force, 10); // For visualization purposes only
        Position.Update(Position.X + (force.X * (dt/100)), Position.Y + (force.Y * (dt/100)));
    }
}

public class DroneOversight : Point
{
    private readonly double _finishX;
    private readonly double _finishY;
    private double _speed = 10.0;

    public DroneOversight(double maxX, double maxY, int id, double finishX, double finishY) : base(maxX, maxY, id)
    {
        _finishX = finishX;
        _finishY = finishY;
    }

    public override void UpdatePosition(Coordinate sheepCentroid, double dt, double[] settings)
    {
        _speed = settings[2];
        var force = new Vector2(0, 0);
        var sheepVcentroid = new Vector2(Convert.ToSingle(Position.X - sheepCentroid.X), Convert.ToSingle(Position.Y - sheepCentroid.Y));
        var sheepVcentroidReduced = Vector2.Divide(sheepVcentroid, 5);
        var negated = Vector2.Negate(sheepVcentroidReduced);
        force = Vector2.Add(force, negated);
        
        
        var sheepVfinish = new Vector2(Convert.ToSingle(Position.X - _finishX), Convert.ToSingle(Position.Y - _finishY));
        var sheepVfinishNorm = Vector2.Negate(Vector2.Normalize(sheepVfinish));
        force = Vector2.Add(force, Vector2.Multiply(sheepVfinishNorm, (float)_speed));
        
        
        Force = Vector2.Multiply(force, 10); // For visualization purposes only
        Position.Update(Position.X + (force.X * (dt/100)), Position.Y + (force.Y * (dt/100)));
    }
}