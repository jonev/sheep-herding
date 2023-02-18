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
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(2, 0)]
    [InlineData(3, 0)]
    [InlineData(4, 0)]
    [InlineData(5, 0)]
    [InlineData(6, 0)]
    [InlineData(7, 0)]
    [InlineData(9, 0)]
    // [InlineData(10, 0)] Cross - not ready
    // [InlineData(11, 0)] Guard - not ready
    public async Task Test_Path_SimpleSheeps(int pathNr, int randomAngle)
    {
        var loggerMock = new Mock<ILogger>();
        var service = new HerdService(loggerMock.Object, null, "testClient", Globals.RandomSeed, new SheepSettings());
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