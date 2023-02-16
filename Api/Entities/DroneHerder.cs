using System.Numerics;
using SheepHerding.Api.Helpers;

namespace SheepHerding.Api.Entities;

public class DroneHerder : Point
{
    private readonly double _speed;

    public DroneHerder(double maxX, double maxY, int id, double speed) : base(id)
    {
        _speed = speed;
    }

    public void UpdatePosition(Coordinate command)
    {
        var commandVector = Converter.ToVector2(Position, command);
        var commandVectorSpeedLimited = Vector2.Multiply(Vector2.Normalize(commandVector), (float) _speed);
        Force = Vector2.Multiply(commandVectorSpeedLimited, 10); // For visualization purposes only
        Position.Update(Position.X + commandVectorSpeedLimited.X, Position.Y + commandVectorSpeedLimited.Y);
    }
}