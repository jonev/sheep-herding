namespace SheepHerding.Api.StateMachine;

public enum Trigger
{
    Start,
    NewHerdInRange,
    NewHerdCollected,
    SheepEscaped,
    SheepCaptured,
    CommandsExecuted,
    PathPointOutOfRange,
    AllSheepsAtFinish,
}