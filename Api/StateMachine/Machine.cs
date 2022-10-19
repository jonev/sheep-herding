using Stateless;

namespace SheepHerding.Api.StateMachine;

public class Machine
{
    private readonly StateMachine<State,Trigger> _machine = new (State.Start);
    public State State => _machine.State;
    public Machine()
    {
        _machine.Configure(State.Start)
            .PermitIf(Trigger.Start, State.FetchingFirstHerd);
        
        _machine.Configure(State.FetchingFirstHerd)
            .PermitIf(Trigger.NewHerdCollected, State.FollowPath);
        
        _machine.Configure(State.FetchingNewHerd)
            .Permit(Trigger.NewHerdCollected, State.FollowPath);

        _machine.Configure(State.FollowPath)
            .Permit(Trigger.NewHerdInRange, State.FetchingNewHerd)
            .Permit(Trigger.SheepEscaped, State.RecollectSheep)
            .Permit(Trigger.CommandsExecuted, State.Waiting)
            .Permit(Trigger.AllSheepsAtFinish, State.Finished);

        _machine.Configure(State.RecollectSheep)
            .Permit(Trigger.SheepCaptured, State.FollowPath);
        
        _machine.Configure(State.Waiting)
            .Permit(Trigger.PathPointOutOfRange, State.FollowPath);
    }

    public void Fire(State inState, Trigger trigger, Func<bool> guard)
    {
        if (_machine.State == inState && guard())
        {
            _machine.Fire(trigger);
        }
    }

    public void ExecuteOnEntry(State state, Action action)
    {
       _machine.Configure(state)
            .OnEntry(t => action());
    }
}