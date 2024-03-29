namespace SheepHerding.Api.StateMachine;

public enum Trigger
{
    Start,
    NewHerdInRange,
    NewHerdCollected,
    SheepEscaped,
    SheepCaptured,
    CommandsExecuted,
    CornerApproaching,
    LeftCorner,
    IntersectionApproaching,
    IntersectionAvoided,
    PathPointOutOfRange,
    AllSheepsAtFinish
}