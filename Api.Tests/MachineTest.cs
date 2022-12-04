using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SheepHerding.Api.StateMachine;
using Xunit.Abstractions;

namespace SheepHerding.Api.Tests;

public class MachineTest
{
    private readonly ITestOutputHelper _output;

    public MachineTest(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void Test()
    {
        var loggerMock = new Mock<ILogger>();
        var m = new Machine(loggerMock.Object);
        m.State.Should().Be(State.Start);
        
        m.Fire(State.Start, Trigger.Start, () => true);
        m.State.Should().Be(State.FetchingFirstHerd);
        
        m.Fire(State.FetchingFirstHerd, Trigger.NewHerdCollected, () => true);
        m.State.Should().Be(State.FollowPathStraight);
        
        m.Fire(State.FollowPathStraight, Trigger.CornerApproaching, () => true);
        m.State.Should().Be(State.FollowPathCorner);
        
        m.Fire(State.FollowPathCorner, Trigger.LeftCorner, () => true);
        m.State.Should().Be(State.FollowPathStraight);
    }
}