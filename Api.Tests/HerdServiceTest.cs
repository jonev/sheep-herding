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
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(3, 1)]
    [InlineData(4, 1)]
    [InlineData(5, 1)]
    [InlineData(6, 1)]
    [InlineData(7, 1)]
    [InlineData(9, 1)]
    [InlineData(10, 1)] // Cross
    // [InlineData(11, 0)] Guard - not started on
    [InlineData(0, 20)]
    [InlineData(1, 20)]
    [InlineData(2, 20)]
    [InlineData(3, 20)]
    [InlineData(4, 20)]
    [InlineData(5, 20)]
    [InlineData(6, 20)]
    [InlineData(7, 20)]
    [InlineData(9, 20)]
    [InlineData(10, 20)] // Cross
    // [InlineData(11, 0)] Guard - not started on
    [InlineData(0, 40)]
    [InlineData(1, 40)]
    [InlineData(2, 40)]
    [InlineData(3, 40)]
    [InlineData(4, 40)]
    [InlineData(5, 40)]
    [InlineData(6, 40)]
    [InlineData(7, 40)]
    [InlineData(9, 40)]
    [InlineData(10, 40)] // Cross
    // [InlineData(11, 0)] Guard - not started on
    [InlineData(0, 60)]
    [InlineData(1, 60)]
    [InlineData(2, 60)]
    [InlineData(3, 60)]
    [InlineData(4, 60)]
    [InlineData(5, 60)]
    [InlineData(6, 60)]
    [InlineData(7, 60)]
    [InlineData(9, 60)]
    [InlineData(10, 60)] // Cross
    // [InlineData(11, 0)] Guard - not started on
    // Harder random angle
    [InlineData(0, 70)]
    [InlineData(1, 70)]
    [InlineData(2, 70)]
    [InlineData(3, 70)]
    [InlineData(4, 70)]
    [InlineData(5, 70)]
    [InlineData(6, 70)]
    [InlineData(7, 70)]
    [InlineData(9, 70)]
    [InlineData(10, 70)] // Cross
    // [InlineData(11, 0)] Guard - not started on
    public async Task Test_Path_SimpleSheeps(int pathNr, int randomAngle)
    {
        var loggerMock = new Mock<ILogger>();
        var service = new HerdService(loggerMock.Object, null, "testClient", Globals.RandomSeed, new SheepSettings());
        try
        {
            service.InterceptCross = true;
            service.PathNr = pathNr;
            service.Connected = true;
            service.Reset = false;
            service.Start = true;
            service.VisualizationSpeed = 0;
            service.FailedTimout = 2.0;
            service.RandomFactor = randomAngle;
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