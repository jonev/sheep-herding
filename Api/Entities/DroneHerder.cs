using System.Numerics;

namespace SheepHerding.Api.Entities;

public class DroneHerder : Point
{
    private readonly DroneOversight _oversight;

    public DroneHerder(double maxX, double maxY, int id, DroneOversight oversight) : base(maxX, maxY, id)
    {
        _oversight = oversight;
    }

    public override void UpdatePosition(Coordinate sheepCentroid, double dt, double[] settings)
    {
        var force = new Vector2(0, 0);
        
        var commandVector = Vector2.Negate(new Vector2(Convert.ToSingle(Position.X - _oversight.HerdCommands[Id].X), Convert.ToSingle(Position.Y - _oversight.HerdCommands[Id].Y)));
        var commandVectorReduced = Vector2.Divide(commandVector, 3);

        force = Vector2.Add(force, commandVectorReduced);
        
        Force = Vector2.Multiply(force, 10); // For visualization purposes only
        Position.Update(Position.X + (force.X * (dt/100)), Position.Y + (force.Y * (dt/100)));
    }
}