using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SheepHerding.Api.Entities;
using SheepHerding.Api.Services;
using Xunit.Abstractions;

namespace SheepHerding.Api.Tests;

public class HerdServiceTest
{
    private readonly ITestOutputHelper _output;

    public HerdServiceTest(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 20)]
    [InlineData(2, 20)]
    [InlineData(3, 20)]
    [InlineData(4, 20)]
    [InlineData(5, 20)]
    [InlineData(0, 10)]
    [InlineData(1, 10)]
    [InlineData(2, 10)]
    [InlineData(3, 10)]
    [InlineData(4, 10)]
    [InlineData(5, 10)]
    [InlineData(0, 5)]
    [InlineData(1, 5)]
    [InlineData(2, 5)]
    [InlineData(3, 5)]
    [InlineData(4, 5)]
    [InlineData(5, 5)]
    [InlineData(0, 4)]
    [InlineData(1, 4)]
    [InlineData(2, 4)]
    [InlineData(3, 4)]
    [InlineData(4, 4)]
    [InlineData(5, 4)]
    [InlineData(0, 3)]
    //[InlineData(1, 3)] Disabled for now
    [InlineData(2, 3)]
    //[InlineData(3, 3)] Disabled for now
    [InlineData(4, 3)]
    [InlineData(5, 3)]
    public async Task Test_Path_SimpleSheeps(int pathNr, int randomAngle)
    {
        var loggerMock = new Mock<ILogger>();
        var service = new HerdService(loggerMock.Object,  null, "testClient", Globals.RandomSeed, new SheepSettings());
        try
        {
            service.PathNr = pathNr;
            service.Connected = true;
            service.Reset = false;
            service.Start = true;
            service.VisualizationSpeed = 0;
            service.FailedTimout = 2.0;
            service.RandomAngle = randomAngle;
            await service.InitializeAndRun();
            var sheeps = service.Sheeps;
            var nrOfSheepsFinished = sheeps.Where(s => s.IsInsideFinishZone()).Count();
            _output.WriteLine($"Path: {service.PathNr}, Finished: {nrOfSheepsFinished}/{sheeps.Count}");
            service.Finished.Should().BeTrue();
        }
        finally
        {
            service.Dispose();
        }
    }
}