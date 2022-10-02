using System.Numerics;
using SheepHerding.Api.Helpers;

namespace SheepHerding.Api.Entities;

public class DroneHerder : Point
{
    private readonly double _speed;

    public DroneHerder(double maxX, double maxY, int id, double speed) : base(maxX, maxY, id)
    {
        _speed = speed;
    }

    public void UpdatePosition(double dt, Coordinate command)
    {
        var force = new Vector2(0, 0);

        var commandVector = Converter.ToVector2(Position, command);
        var commandVectorSpeedLimited = Vector2.Multiply(Vector2.Normalize(commandVector), (float) _speed);
        // var commandVectorReduced = Vector2.Divide(commandVector, 5);

        force = Vector2.Add(force, commandVectorSpeedLimited);
        
        Force = Vector2.Multiply(force, 10); // For visualization purposes only
        Position.Update(Position.X + (force.X * (dt/100)), Position.Y + (force.Y * (dt/100)));
    }
}