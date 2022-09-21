namespace SheepHerding.Api.StateMachine;

public enum Trigger
{
    NewHerdInRange,
    NewHerdCollected,
    SheepEscaped,
    SheepCaptured,
    AllSheepsAtFinish,
}