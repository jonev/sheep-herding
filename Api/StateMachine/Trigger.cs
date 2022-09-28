namespace SheepHerding.Api.StateMachine;

public enum Trigger
{
    Start,
    NewHerdInRange,
    NewHerdCollected,
    SheepEscaped,
    SheepCaptured,
    PathPointInRange,
    PathPointOutOfRange,
    AllSheepsAtFinish,
}