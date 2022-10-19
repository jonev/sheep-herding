namespace SheepHerding.Api.StateMachine;

public enum State
{
    Start,
    FetchingFirstHerd,
    FetchingNewHerd,
    FollowPath,
    RecollectSheep,
    Waiting,
    Finished,
}