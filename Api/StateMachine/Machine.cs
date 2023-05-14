using Stateless;

namespace SheepHerding.Api.StateMachine;

public class Machine
{
    private readonly ILogger _logger;
    private readonly StateMachine<State, Trigger> _machine = new(State.Start);

    public Machine(ILogger logger)
    {
        _logger = logger;
        _machine.Configure(State.Start)
            .PermitIf(Trigger.Start, State.FetchingFirstHerd);

        _machine.Configure(State.FetchingFirstHerd)
            .PermitIf(Trigger.NewHerdCollected, State.FollowPath);

        _machine.Configure(State.FollowPath)
            .Permit(Trigger.CommandsExecuted, State.AckPathCoordinator)
            .Permit(Trigger.AllSheepsAtFinish, State.Finished)
            .InitialTransition(State.FollowPathStraight);

        _machine.Configure(State.FollowPathStraight)
            .SubstateOf(State.FollowPath)
            .Permit(Trigger.IntersectionApproaching, State.FollowPathIntersectionLeft);

        _machine.Configure(State.FollowPathIntersectionLeft)
            .SubstateOf(State.FollowPath)
            .Permit(Trigger.IntersectionAvoided, State.FollowPath);

        _machine.Configure(State.AckPathCoordinator)
            .Permit(Trigger.PathPointOutOfRange, State.FollowPath);
    }

    public State State => _machine.State;

    public void Fire(State inState, Trigger trigger, Func<bool> guard)
    {
        if (_machine.IsInState(inState) && guard())
            _machine.Fire(trigger);
    }

    public void Fire(Trigger trigger, Func<bool> guard)
    {
        if (guard())
            _machine.Fire(trigger);
    }

    public void ExecuteOnEntry(State state, Action action)
    {
        _machine.Configure(state)
            .OnEntry(t => action());
    }
}