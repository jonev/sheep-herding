namespace SheepHerding.Api.Entities;

public class SheepSettings
{
    public double NeighborToCloseStartMoveThreshold { get; set; } = 25.0;
    public double NeighborToFarStartToMoveThreshold { get; set; } = 150.0;
    public double CentroidOfHerdToFarStartMoveThreshold { get; set; } = 100.0;
    public double CentroidOfHerdToFarEndMoveThreshold { get; set; } = 200.0;
    public double EnemyToCloseStartMoveThreshold { get; set; } = 100.0;
    public float SpeedAdjustment { get; set; } = 2.5f;
    public float PersonalSpaceForce { get; set; } = 3.0f;
    // public float HoldTogetherForce { get; set; } = 1.0f;
    // public float RunAwayForce { get; set; } = 1.0f;
    
    // >>>--- Force input = Need to be in total 1.0f ---
    // public float PersonalSpaceForce { get; set; } = 0.4f;
    // public float HoldTogetherForce { get; set; } = 0.2f;
    // public float RunAwayForce { get; set; } = 0.4f;
    // --- End Force input ------<<<<

    public double RandomAngleRange { get; set; } = Math.PI / 3;
    public int RandomAngleUpdateDelayFactor { get; set; } = 10;
}