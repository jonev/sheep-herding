namespace SheepHerding.Api.Entities;

public class SheepSettings
{
    public double NeighborToCloseStartMoveThreshold { get; set; } = 25.0;
    public double NeighborToFarStartToMoveThreshold { get; set; } = 150.0;
    public double CentroidOfHerdToFarStartMoveThreshold { get; set; } = 100.0;
    public double CentroidOfHerdToFarEndMoveThreshold { get; set; } = 200.0;
    public double EnemyToCloseStartMoveThreshold { get; set; } = 100.0;
    public double MaxSpeed { get; set; } = 100.0;
    public float PersonalSpaceForce { get; set; } = 3.0f;
    public float HoldTogetherForce { get; set; } = 1.0f;
    public float RunAwayForce { get; set; } = 1.0f;

    public double RandomAngleRange { get; set; } = Math.PI / 3;
    public int RandomAngleUpdateDelayFactor { get; set; } = 10;
}