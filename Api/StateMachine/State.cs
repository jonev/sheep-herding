namespace SheepHerding.Api.StateMachine;

public enum State
{
    FetchingFirstHerd,
    FetchingNewHerd,
    FollowPath,
    RecollectSheep,
    Finished,
}