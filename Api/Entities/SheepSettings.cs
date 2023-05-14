namespace SheepHerding.Api.Entities;

public class SheepSettings
{
    public double NeighborToCloseStartMoveThreshold { get; set; } = 15.0;
    public double CentroidOfHerdToFarStartMoveTowardHerdThreshold { get; set; } = 100.0;
    public double CentroidOfHerdToFarEndMoveThreshold { get; set; } = 200.0;
    public double EnemyToCloseStartMoveThreshold { get; set; } = 100.0; // DO not Change

    // >>>--- Force input = Need to be in total 1.0f ---
    public float PersonalSpaceForce { get; set; } = 0.3f;
    public float HoldTogetherForce { get; set; } = 0.1f;
    public float RunAwayForce { get; set; } = 1.0f;

    public float IntersectionApproachingForce { get; set; } = 1.1f;
    // --- End Force input ------<<<<

    public double RandomAngleAddedToForce { get; set; } = Math.PI / 100.0;
    public int RandomAngleUpdateDelayFactor { get; set; } = 200;
}