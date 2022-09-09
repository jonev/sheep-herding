using System.Numerics;
using SheepHerding.Api.Helpers;

namespace SheepHerding.Api.Entities;

public class DroneHerder : Point
{
    private readonly DroneOversight _oversight;

    public DroneHerder(double maxX, double maxY, int id, DroneOversight oversight) : base(maxX, maxY, id)
    {
        _oversight = oversight;
    }

    public void UpdatePosition(double dt)
    {
        var force = new Vector2(0, 0);

        var commandVector = Converter.ToVector2(Position, _oversight.HerdCommands[Id]);
        var commandVectorReduced = Vector2.Divide(commandVector, 3);

        force = Vector2.Add(force, commandVectorReduced);
        
        Force = Vector2.Multiply(force, 10); // For visualization purposes only
        Position.Update(Position.X + (force.X * (dt/100)), Position.Y + (force.Y * (dt/100)));
    }
}