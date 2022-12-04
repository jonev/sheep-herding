namespace SheepHerding.Api.StateMachine;

public enum State
{
    Start,
    FetchingFirstHerd,
    FetchingNewHerd,
    FollowPath,
    FollowPathCorner,
    FollowPathStraight,
    RecollectSheep,
    Waiting,
    Finished,
}